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
                _client.SendCommand("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />");

                _timer.Start();
            }
        }

        // Esegue questa operazione ogni volta che il timer esegue un tick (impostato a 150Hz nel nostro caso)
        private void OnTick(object sender, EventArgs e)
        {
            // 1. Recupera i dati grezzi XML dal buffer del client
            List<string> packets = _client.ReadData();

            foreach (string packet in packets)
            {
                // 2. Trasforma l'XML in un oggetto GPData (Grezzo)
                GPData rawData = GPParser.Parse(packet);

                // 3. Pulizia del dato dal rumore (blinks e bordi display)
                GPData validData = _validationFilter.ValidationFilter(rawData);

                // Se il filtro gestisce un dato valido
                if (validData != null)
                {
                    GPData smoothData = _smoothingFilter.AdaptiveSmoothing(validData);

                    // 1. Coordinate Fisiche (per Windows/Hit-Test)
                    var (physX, physY) = GPConverter.ToPhysicalScreenPoint(smoothData);

                    // 2. Coordinate Logiche (per la UI/GazeCursor) usando la nuova funzione
                    var (logX, logY) = GPConverter.ToLogicalScreenPoint(smoothData);

                    // Aggiorniamo l'oggetto GazeCursor
                    GazeCursor.X = logX;
                    GazeCursor.Y = logY;

                    // 3. Logica di Hit-Testing (usa physX/physY)
                    if (Application.Current.MainWindow is Window window)
                    {
                        Point windowPoint = GPConverter.ToWindowPoint(new Point(physX, physY), window);
                        _dwellManager.Update(_targetProvider.GetElementAtPoint(windowPoint, window));
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



