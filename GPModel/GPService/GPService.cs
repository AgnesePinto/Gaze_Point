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
            _cursorController.Show();
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

        public void ResetInteractionState()
        {
            _dwellManager.Clear();
            _lastSelectedElement = null;

            if (Application.Current.MainWindow is Window window)
            {
                _targetProvider.ForceRefreshCache(window);
            }
        }

        public void UpdateWindowContext(Window newWindow)
        {
            _targetProvider.ForceRefreshCache(newWindow);
            ResetInteractionState();
        }


        public void RefreshInteractionTargets()
        {
            if (Application.Current.MainWindow is Window window)
            {
                _targetProvider.InvalidateCache(); 
                _targetProvider.ForceRefreshCache(window);
            }
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
                if (rawData == null) continue; 

                GPData validData = _validationFilter.ValidationFilter(rawData);
                if (validData != null)
                {
                    lastValidData = _smoothingFilter.AdaptiveSmoothing(validData);
                    lastRawData = rawData;

                    var (logX, logY) = GPConverter.ToLogicalScreenPoint(lastValidData);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _cursorController.UpdatePosition(logX, logY);
                    }), DispatcherPriority.Render);
                }
            }

            if (lastValidData != null && lastRawData != null)
            {
                bool isSaccade = _saccadeDetector.IsSignificantSaccade(lastRawData);

                double staticX = lastValidData.BPOGX;
                double staticY = lastValidData.BPOGY;

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var currentWin = Application.Current.MainWindow;
                    if (currentWin == null || !currentWin.IsVisible) return;

                    try
                    {

                        double winX = staticX * SystemParameters.PrimaryScreenWidth;
                        double winY = staticY * SystemParameters.PrimaryScreenHeight;
                        Point screenPt = new Point(winX, winY);

                        Point winPt = currentWin.PointFromScreen(screenPt);

                        if (isSaccade)
                        {
                            _dwellManager.Update(null);
                        }
                        else
                        {
                            FrameworkElement target = _targetProvider.GetElementAtPoint(winPt, currentWin);
                            _dwellManager.Update(target);
                        }
                    }
                    catch {  }
                }), DispatcherPriority.Input);
            }
        }


        public void Stop()
        {
            _timer.Stop();
            _client.SendCommand("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />");
            _client.Disconnect();
            Application.Current.Dispatcher.Invoke(() => _cursorController.Hide());
        }
    }
}