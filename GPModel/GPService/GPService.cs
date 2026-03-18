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
        private readonly GPCursorController _cursorController;
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
            _cursorController = new GPCursorController();

#if DEBUG
            IsCursorVisible = true;
            _cursorController.Show();	// Mostriamo la finestra esterna
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
            if (packets.Count == 0) return;

            GPData lastValidData = null;
            GPData lastRawData = null;

            foreach (string packet in packets)
            {
                GPData rawData = GPParser.Parse(packet);
                GPData validData = _validationFilter.ValidationFilter(rawData);

                if (validData != null)
                {
                    lastValidData = _smoothingFilter.AdaptiveSmoothing(validData);
                    lastRawData = rawData;

                    // 1. MOVIMENTO CURSORE (Per ogni pacchetto)
                    var (logX, logY) = GPConverter.ToLogicalScreenPoint(lastValidData);
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _cursorController.UpdatePosition(logX, logY);
                    }), DispatcherPriority.Render);
                }
            }

            // 2. LOGICA DI INTERAZIONE (Solo sull'ultimo pacchetto)
            if (lastValidData != null && lastRawData != null)
            {
                // Calcoliamo la saccade QUI, prima di passarla al Dispatcher
                bool isSaccade = _saccadeDetector.IsSignificantSaccade(lastRawData);

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Application.Current.MainWindow is Window window)
                    {
                        var (physX, physY) = GPConverter.ToPhysicalScreenPoint(lastValidData);
                        Point windowPoint = GPConverter.ToWindowPoint(new Point(physX, physY), window);

                        if (isSaccade)
                        {
                            _dwellManager.Update(null);
                        }
                        else
                        {
                            // Chiamata sicura al thread UI per trovare l'elemento (incluse le popup)
                            FrameworkElement target = _targetProvider.GetElementAtPoint(windowPoint, window);
                            _dwellManager.Update(target);
                        }
                    }
                }), DispatcherPriority.Input);
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

        public void RefreshInteractionTargets()
        {
            if (Application.Current.MainWindow is Window window)
            {
                // Forziamo il provider a ricalcolare tutto (inclusi i nuovi popup)
                _targetProvider.ForceRefreshCache(window);
            }
        }

        public void Stop()
        {
            _timer.Stop();
            _client.SendCommand("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />");
            _client.Disconnect();

            // Nascondi il cursore alla chiusura
            Application.Current.Dispatcher.Invoke(() => _cursorController.Hide());
        }
    }
}









