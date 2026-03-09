using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Gaze_Point.Connection;
using Gaze_Point.GPModel.GPRecord;
using System.Collections.Generic;


namespace Gaze_Point.Services
{
    public class GPService
    {
        [DllImport("user32.dll")]       // Importazione API Windows per forzare la posizione del cursore fisico
        private static extern bool SetCursorPos(int x, int y);

        private readonly GPClient _client;
        private readonly DispatcherTimer _timer;
        private readonly GPValidationFilter _validationFilter;

        public event Action<GPData> OnDataReceived;            // Evento che allerta dell'arrivo di un nuovo punto dello sguardo per passare i dati al ViewModel

        public GPService()
        {
            _client = new GPClient();

            _validationFilter = new GPValidationFilter();

            // Setup del timer (esegue il Tick sul thread UI per 150 volte al secondo)
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(15); // 150Hz
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

        // Esegue questa operazione ogni volta che il timer esegue un tick (impostato a 150Hz nel nostro caso
        private void OnTick(object sender, EventArgs e)
        {
            // 1. Recupera i dati grezzi XML dal buffer del client
            List<string> packets = _client.ReadData();

            foreach (string packet in packets)
            {
                // 2. Trasforma l'XML in un oggetto GPData (Grezzo)
                GPData rawData = GPParser.Parse(packet);

                // 3. PASSO FONDAMENTALE: Sanificazione del dato
                // Il filtro gestisce internamente i blink e i confini del display
                GPData validData = _validationFilter.ValidationFilter(rawData);

                // Se il filtro restituisce un dato valido (non nullo)
                if (validData != null)
                {
                    // 4. Conversione in pixel fisici (ora senza ridondanze nel Converter)
                    var (physX, physY) = GPConverter.ToPhysicalScreenPoint(validData);

                    // 5. Spostamento fisico del cursore del mouse
                    SetCursorPos(physX, physY);

                    // 6. Notifica agli altri componenti dell'applicazione
                    OnDataReceived?.Invoke(validData);
                }
            }
        }

        public void Stop()
        {
            if (_timer.IsEnabled)
                _timer.Stop();

            _client.Disconnect();
        }
    }
}


