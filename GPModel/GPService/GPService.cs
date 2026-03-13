using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Gaze_Point.Connection;
using Gaze_Point.GPModel.GPRecord;
using System.Collections.Generic;
using Gaze_Point.GPModel.GPInteraction;
using System.Windows;
using Gaze_Point.GPModel.GPCursor;
using System.Windows.Input;

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
        public GPCursor GazeCursor { get; } = new GPCursor();
        public bool IsCursorVisible { get; }

        public event Action<GPData> OnDataReceived;     // Evento che allerta dell'arrivo di un nuovo punto dello sguardo per passare i dati al ViewModel
        public event Action<FrameworkElement> OnElementFocused;     // Evento che allerta ViewModel quando un elemento è stato fissato con successo

        public GPService()
        {
            _client = new GPClient();
            _validationFilter = new GPValidationFilter();
            _smoothingFilter = new GPSmoothingFilter();
            _targetProvider = new GPTargetProvider();   
            _dwellManager = new GPDwellManager();
            _saccadeDetector = new GPSaccadeDetector();

#if DEBUG
            IsCursorVisible = true;  // In Debug il cursore è visibile
#else
                IsCursorVisible = false; // In Release il cursore è nascosto
#endif

            _dwellManager.OnElementFocused += (element) => OnElementFocused?.Invoke(element);      // Se il dwellManager capisce che un elemento è fissato, avvisa i listeners

            // Setup del timer (esegue il Tick sul thread UI per 150 volte al secondo)
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(6.6);           // Circa 150Hz
            _timer.Tick += OnTick;
        }

        public void Start()
        {
            _client.Connect();

            if (_client.IsConnected)
            {

                // Comandi Gazepoint per attivare l'invio dei dati POG
                _client.SendCommand("<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />");
                _client.SendCommand("<SET ID=\"ENABLE_SEND_SACCADE\" STATE=\"1\" />");
                _client.SendCommand("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />");

                _timer.Start();
            }
        }

        // Esegue questa operazione ogni volta che il timer esegue un tick (impostato a 150Hz nel nostro caso)
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

                    // 1. Coordinate Logiche per gaze cursor
                    var (logX, logY) = GPConverter.ToLogicalScreenPoint(smoothData);
                    GazeCursor.X = logX;
                    GazeCursor.Y = logY;

                    // 1. Controllo se c'è uno spostamento rapido (Saccade)
                    bool isSaccade = _saccadeDetector.IsSignificantSaccade(rawData);

                    if (Application.Current.MainWindow is Window window)
                    {
                        var (physX, physY) = GPConverter.ToPhysicalScreenPoint(smoothData);
                        Point windowPoint = GPConverter.ToWindowPoint(new Point(physX, physY), window);

                        if (isSaccade)
                        {
                            // Se l'occhio salta, resettiamo il tempo di fissazione
                            _dwellManager.Update(null);
                        }
                        else
                        {
                            // Solo se lo sguardo è "calmo" cerchiamo l'elemento UI
                            FrameworkElement target = _targetProvider.GetElementAtPoint(windowPoint, window);
                            _dwellManager.Update(target);
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

            }

            _client.Disconnect();
        }
    }
}



