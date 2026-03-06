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
        // Importazione API Windows per muovere il cursore fisico
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        private readonly GPClient _client;
        private readonly DispatcherTimer _timer;

        // Evento per passare i dati al ViewModel
        public event Action<GPData> OnDataReceived;

        public GPService()
        {
            _client = new GPClient();

            // Setup del timer (esegue il Tick sul thread UI)
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10); // 100Hz
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

        // Esegue questa operazione ogni volta che il timer esegue un tick (impostato a 100Hz nel nostro caso
        private void OnTick(object sender, EventArgs e)
        {
            List<string> packets = _client.ReadData();       // Recupera i dati grezzi XML ricevuti dall'eye tracker

            foreach (string packet in packets)
            {
                GPData data = GPParser.Parse(packet);       // Trasforma i packet XML in un oggetto C# facile da leggere

                if (data != null && data.IsValid)
                {
                    // 1. Muove il cursore fisico usando i pixel calcolati dal Converter
                    var (physX, physY) = GPConverter.ToPhysicalScreenPoint(data);           // Converto le coodinate dell'eye tracker in pixel
                    SetCursorPos(physX, physY);                                             // Passo al cursore le coordinate convertite

                    // 2. Notifica il ViewModel per eventuali aggiornamenti grafici
                    OnDataReceived?.Invoke(data);
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


