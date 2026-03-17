using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Gaze_Point.Connection;
using Gaze_Point.GPModel.GPRecord;
using Gaze_Point.GPModel.GPInteraction;
using Gaze_Point.GPModel.GPCursor;

namespace Gaze_Point.Services
{
    public class GPService
    {
        private readonly GPClient _client;
        private readonly DispatcherTimer _timer;
        private readonly GPValidationFilter _validationFilter;
        private readonly GPSmoothingFilter _smoothingFilter;
        private readonly GPTargetProvider _targetProvider;
        private readonly GPDwellManager _dwellManager;
        private readonly GPSaccadeDetector _saccadeDetector;
        private readonly GPTargetLocker _targetLocker;
        private readonly GPMovementDetector _movementDetector;
        private FrameworkElement _lastSelectedElement;

        public GPCursor GazeCursor { get; } = new GPCursor();
        public bool IsCursorVisible { get; }

        public event Action<FrameworkElement> OnElementFocused;

        public GPService()
        {
            _client = new GPClient();
            _validationFilter = new GPValidationFilter();
            _smoothingFilter = new GPSmoothingFilter();
            _targetProvider = new GPTargetProvider();
            _dwellManager = new GPDwellManager();
            _saccadeDetector = new GPSaccadeDetector();
            _targetLocker = new GPTargetLocker();
            _movementDetector = new GPMovementDetector();

#if DEBUG
            IsCursorVisible = true;
#else
            IsCursorVisible = false; 
#endif

            _dwellManager.OnElementFocused += (element) =>
            {
                _lastSelectedElement = element;
                _targetLocker.Activate();
                OnElementFocused?.Invoke(element);
            };

            _targetLocker.OnLockExpired += (points) =>
            {
                var result = _movementDetector.Analyze(points);
                bool foundNext = false;

                if (result.Type == GPMovementDetector.MovementType.SmallStep)
                {
                    if (Application.Current.MainWindow is Window window)
                    {
                        var nextElement = _targetProvider.GetNextElementInDirection(_lastSelectedElement, result.Angle, window);
                        if (nextElement != null)
                        {
                            _lastSelectedElement = nextElement;
                            OnElementFocused?.Invoke(nextElement);
                            _targetLocker.Activate();
                            foundNext = true;
                        }
                    }
                }

                if (!foundNext)
                {
                    _dwellManager.Clear();
                }
            };

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(6.6);
            _timer.Tick += OnTick;
        }

        public void Start()
        {
            _client.Connect();
            if (_client.IsConnected)
            {
                _client.SendCommand("<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />");
                _client.SendCommand("<SET ID=\"ENABLE_SEND_SACCADE\" STATE=\"1\" />");
                _client.SendCommand("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />");
                _timer.Start();
            }
        }

        public void UpdateWindowContext(Window newWindow)
        {
            // Forza il TargetProvider a rifare l'inventario degli elementi immediatamente
            _targetProvider.ForceRefreshCache(newWindow);

            // Pulisce le vecchie sottoscrizioni e i timer di focus
            ClearFocusedElementSubscriptions();
        }

        private void OnTick(object sender, EventArgs e)
        {
            List<string> packets = _client.ReadData();

            foreach (string packet in packets)
            {
                GPData rawData = GPParser.Parse(packet);
                // Il ValidationFilter ora gestisce internamente il Blanking Period
                GPData validData = _validationFilter.ValidationFilter(rawData);

                if (validData != null)
                {
                    GPData smoothData = _smoothingFilter.AdaptiveSmoothing(validData);

                    var (logX, logY) = GPConverter.ToLogicalScreenPoint(smoothData);
                    GazeCursor.X = logX;
                    GazeCursor.Y = logY;

                    if (_targetLocker.IsLocked)
                    {
                        // Passiamo rawData.BPOGV per assicurarci che il Locker 
                        // scarti i punti congelati durante il blink/blanking
                        _targetLocker.ProcessPoint(smoothData.BPOGX, smoothData.BPOGY, rawData.BPOGV);
                    }
                    else
                    {
                        bool isSaccade = _saccadeDetector.IsSignificantSaccade(rawData);

                        if (Application.Current.MainWindow is Window window)
                        {
                            var (physX, physY) = GPConverter.ToPhysicalScreenPoint(smoothData);
                            Point windowPoint = GPConverter.ToWindowPoint(new Point(physX, physY), window);

                            if (isSaccade)
                                _dwellManager.Update(null);
                            else
                            {
                                FrameworkElement target = _targetProvider.GetElementAtPoint(windowPoint, window);
                                _dwellManager.Update(target);
                            }
                        }
                    }
                }
            }
        }

        public void ClearFocusedElementSubscriptions()
        {
            // Rimuove tutti i delegati (ViewModel) attaccati a questo evento
            OnElementFocused = null;

            // Opzionale: Resettiamo anche lo stato interno per la nuova finestra
            _dwellManager.Clear();
            _lastSelectedElement = null;
        }

        public void Stop()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                _client.SendCommand("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />");
            }
            _client.Disconnect();
        }
    }
}









