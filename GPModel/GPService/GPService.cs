using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Gaze_Point.Connection;
using Gaze_Point.GPModel.GPRecord;
using System.Collections.Generic;
using Gaze_Point.GPModel.GPInteraction;
using System.Windows;

namespace Gaze_Point.Services
{
    public class GPService
    {
        // API per spostare il cursore
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        // API per mostrare o nascondere il cursore fisico
        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        private readonly GPClient _client;
        private readonly DispatcherTimer _timer;
        private readonly GPValidationFilter _validationFilter;
        private readonly GPSmoothingFilter _smoothingFilter;
        private readonly GPTargetProvider _targetProvider;
        private readonly GPDwellManager _dwellManager;

        public event Action<GPData> OnDataReceived;     // Evento che allerta dell'arrivo di un nuovo punto dello sguardo per passare i dati al ViewModel
        public event Action<FrameworkElement> OnElementFocused;     // Evento che allerta ViewModel quando un elemento è stato fissato con successo

        public GPService()
        {
            _client = new GPClient();
            _validationFilter = new GPValidationFilter();
            _smoothingFilter = new GPSmoothingFilter();
            _targetProvider = new GPTargetProvider();   
            _dwellManager = new GPDwellManager();

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
                // --- GESTIONE VISIBILITÀ CURSORE ---
#if DEBUG
                // Imposto il cursore come visibile in fase di debug
                ShowCursor(true);
#else
                // Imposto il cursore come invisibile in fase di Release
                ShowCursor(false);
#endif
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
                    // 1. Applichiamo lo smoothing
                    GPData smoothData = _smoothingFilter.AdaptiveSmoothing(validData);

                    // 2. Calcoliamo i PIXEL FISICI (quelli dello schermo intero)
                    var (physX, physY) = GPConverter.ToPhysicalScreenPoint(smoothData);

                    // 3. Muoviamo il cursore reale di Windows
                    SetCursorPos(physX, physY);

                    // 4. Interazione con la finestra
                    if (Application.Current.MainWindow is Window window)
                    {
                        // Creiamo il punto FISICO dove si trova il mouse
                        Point mousePoint = new Point(physX, physY);

                        // Lo convertiamo in punto LOGICO per WPF
                        Point windowPoint = GPConverter.ToWindowPoint(mousePoint, window);

                        // Eseguiamo l'Hit-Test su quel punto preciso
                        FrameworkElement element = _targetProvider.GetElementAtPoint(windowPoint, window);
                        _dwellManager.Update(element);
                    }
                    OnDataReceived?.Invoke(smoothData);
                }
            }
        }

        public void Stop()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();

                // Quando fermiamo il servizio, restituiamo sempre il cursore all'utente
                ShowCursor(true);
            }

            _client.Disconnect();
        }
    }
}



