using System;
using System.Windows.Threading;
using Gaze_Point.Connection;
using Gaze_Point.GPModel.GPRecord;
using System.Collections.Generic;
using Gaze_Point.GPModel.GPInteraction;
using System.Windows;
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

            // Quando il DwellManager rileva un fissaggio, attiva il Locker
            _dwellManager.OnElementFocused += (element) => {
                _lastSelectedElement = element;
                _targetLocker.Activate(); // Il locker si resetta internamente
                OnElementFocused?.Invoke(element);
            };

            // Quando il blocco scade, riceve i punti già filtrati dal Locker
            _targetLocker.OnLockExpired += (points) => {

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

                            // Riattiva il blocco per il nuovo elemento
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

        private void OnTick(object sender, EventArgs e)
        {
            List<string> packets = _client.ReadData();

            foreach (string packet in packets)
            {
                GPData rawData = GPParser.Parse(packet);
                GPData validData = _validationFilter.ValidationFilter(rawData);

                if (validData != null)
                {
                    GPData smoothData = _smoothingFilter.AdaptiveSmoothing(validData);

                    // 1. Aggiornamento cursore visivo
                    var (logX, logY) = GPConverter.ToLogicalScreenPoint(smoothData);
                    GazeCursor.X = logX;
                    GazeCursor.Y = logY;

                    // 2. Se bloccato, deleghiamo la raccolta punti al Locker
                    if (_targetLocker.IsLocked)
                    {
                        _targetLocker.ProcessPoint(smoothData.BPOGX, smoothData.BPOGY, rawData.BPOGV);
                    }
                    else // 3. Se NON bloccato, procediamo con la logica di puntamento standard
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



