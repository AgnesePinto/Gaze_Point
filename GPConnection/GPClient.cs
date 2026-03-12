using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Microsoft.Extensions.Configuration; 

namespace Gaze_Point.Connection
{
    public class GPClient
    {
        // Usiamo i nomi corretti dei campi dichiarati sopra
        private readonly string IpAddress;
        private readonly int IpPort;

        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _buffer = new byte[4096];
        private StringBuilder _dataAccumulator = new StringBuilder();

        public GPClient()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/Connection.json", optional: false, reloadOnChange: true)
                    .Build();

                IpAddress = configuration["IpAddress"] ?? "127.0.0.1";
                IpPort = int.Parse(configuration["IpPort"] ?? "4242");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore nel caricamento della configurazione: " + ex.Message);
                // Valori di fallback in caso di errore
                IpAddress = "127.0.0.1";
                IpPort = 4242;
            }
        }


        // Proprietà per controllo apertura connessione
        public bool IsConnected
        {
            get
            {
                if (_client != null)        // Controllo se l'oggetto esiste
                {
                    return _client.Connected;       // Se esiste restituiamo true come stato di connessione
                }
                return false;           // Se non esiste restituiamo false come stato di connessione
            }
        }


        // Metodo per apertura connessione
        public void Connect()
        {
            try
            {
                if (!IsConnected)            // Se non siamo già collegati
                {
                    _client = new TcpClient(IpAddress, IpPort);          // Iniziaizza il clien e tenta di collegarsi a quell'ip e a quella porta
                    _stream = _client.GetStream();                      // recupera lo stream appena creato

                    Console.WriteLine("Connessione stabilita con successo!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore di connessione: " + ex.Message);
            }

        }


        // Metodo per inviare comandi a Gazepoint
        public void SendCommand(string command)
        {
            if (IsConnected)
            {
                string formattedCommand = command + "\r\n";     // Ogni comando inviato deve terminare con i caratteri a capo
                byte[] data = Encoding.ASCII.GetBytes(formattedCommand);    // Converte una striga di testo in un array di byte (formato richiesto dalla connessione TCP)
                _stream.Write(data, 0, data.Length);        // Invia i dati sul canale di comunicazione 
            }
        }


        // Metodo per la lettura di dati da Gaze Point, impostati in stringhe divise correttamente
        public List<string> ReadData()
        {
            List<string> packets = new List<string>();

            if (IsConnected && _stream.DataAvailable)
            {
                // 1. Leggiamo dallo stream e aggiungiamo allo StringBuilder
                int bytesRead = _stream.Read(_buffer, 0, _buffer.Length);               // Contiene i byte grezzi letti dallo strem
                string rawChunk = Encoding.ASCII.GetString(_buffer, 0, bytesRead);      // Contiene la traduzione in string del pezzo di testo "grezzo" appena arrivato dai bytes
                _dataAccumulator.Append(rawChunk);                                      // Attacca le stringhe grezze a quello che è già presente nello StringBuilder

                // 2. Estraiamo tutti i messaggi completi (delimitati da \r\n)
                string currentContent = _dataAccumulator.ToString();            // Contiene l'intera sequenza di caratteri (grezzi) accumulati fino a quell'istante che viene trasformata in una String (non più StringBuilder)
                int terminatorIndex;

                // Cerchiamo il terminatore finché ce ne sono nel buffer
                while ((terminatorIndex = currentContent.IndexOf("\r\n")) != -1)        // Cerco il primo terminatore, se non ne trovo più, restituisco -1
                {
                    // Estraiamo il pacchetto (senza \r\n)
                    string packet = currentContent.Substring(0, terminatorIndex);
                    if (!string.IsNullOrEmpty(packet))
                    {
                        packets.Add(packet);
                    }

                    // Rimuoviamo il pacchetto processato e il terminatore dallo StringBuilder
                    _dataAccumulator.Remove(0, terminatorIndex + 2);

                    // Aggiorniamo la stringa di controllo per il prossimo ciclo del while
                    currentContent = _dataAccumulator.ToString();
                }
            }

            return packets;
        }


        // Metodo per la chiusura della connessione
        public void Disconnect()
        {
            if (_stream != null)
            {
                _stream.Close();
            }

            if (_client != null)
            {
                _client.Close();
            }

            Console.WriteLine("Connessione chiusa con successo!");
        }
    }
}
