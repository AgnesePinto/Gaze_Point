using Gaze_Point.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Gaze_Point.MainTest
{
    class StringReaderTest
    {
        static void Main(string[] args)
        {
            GPClient gaze = new GPClient();

            // 1. Test Connessione
            gaze.Connect();

            if (gaze.IsConnected)
            {
                // 2. Test Invio Comandi (Abilitazione dati)
                Console.WriteLine("Invio configurazione...");
                gaze.SendCommand("<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />");
                gaze.SendCommand("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />");

                Console.WriteLine("In ascolto per 10 secondi...");
                DateTime endTime = DateTime.Now.AddSeconds(10);

                while (DateTime.Now < endTime)
                {
                    // 3. Test Lettura Multi-pacchetto
                    List<string> pacchetti = gaze.ReadData();

                    foreach (string p in pacchetti)
                    {
                        // Stampiamo solo i dati validi (es. quelli che iniziano con <REC)
                        if (p.StartsWith("<REC"))
                        {
                            Console.WriteLine($"[DATO] {p}");
                        }
                        else
                        {
                            Console.WriteLine($"[INFO/ACK] {p}");
                        }
                    }

                    Thread.Sleep(5); // Frequenza di campionamento tipica Gazepoint: 60Hz-150Hz
                }

                gaze.Disconnect();
            }
            else
            {
                Console.WriteLine("ERRORE: Verifica che Gazepoint Control sia in esecuzione sulla porta 4242.");
            }

            Console.WriteLine("Test terminato. Premi un tasto.");
            Console.ReadKey();
        }
    }
}


