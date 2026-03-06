using System;
using System.Globalization;
using System.Collections.Generic;
using Gaze_Point.GPModel.GPRecord;


namespace Gaze_Point.GPModel.GPRecord
{
    public class GPParser
    {
        /// <summary>
        /// Trasforma una stringa XML grezza di Gazepoint in un oggetto GPData.
        /// </summary>
        public static GPData Parse(string xml)
        {
            // Filtro: processiamo solo i messaggi di tipo RECORD, non gli ACK
            if (string.IsNullOrWhiteSpace(xml) || !xml.StartsWith("<REC"))
                return null;

            GPData data = new GPData();

            // Estrazione attributi principali
            data.FPOGX = GetAttributeValue(xml, "FPOGX");
            data.FPOGY = GetAttributeValue(xml, "FPOGY");
            data.BPOGX = GetAttributeValue(xml, "BPOGX");
            data.BPOGY = GetAttributeValue(xml, "BPOGY");
            data.BPOGV = (int)GetAttributeValue(xml, "BPOGV");
            data.TIME = GetAttributeValue(xml, "TIME");

            // Saccadi (opzionali nel flusso dati)
            data.SACCADE_MAG = GetAttributeValue(xml, "SACCADE_MAG");
            data.SACCADE_DIR = GetAttributeValue(xml, "SACCADE_DIR");

            return data;
        }

        /// <summary>
        /// Estrae il valore numerico di un attributo XML (es. FPOGX="0.5")
        /// </summary>
        private static double GetAttributeValue(string xml, string attributeName)
        {
            string searchTag = attributeName + "=\"";          
            int start = xml.IndexOf(searchTag);                 // Cerca nella stringa xml la posizione dell'attributo seguito dai caratteri ="

            if (start == -1) return 0.0;                        // Se non trova questo attributo =" allora ritorna 0.0

            start += searchTag.Length;                          
            int end = xml.IndexOf("\"", start);                 // Se la trova cerca la fine, cioè le virgolette successive

            if (end == -1) return 0.0;                          // Se non trova la fine restituisce 0.0

            string valueString = xml.Substring(start, end - start);         // Se la trova prende quello che c'è in mezzo alle virgolette, cioè il valore cercato

            // Importante: InvariantCulture gestisce il punto "." come separatore decimale 
            // double.TryParse prova a trasformare il testo in un numero: se il testo è vuoto o sporco, restituisce 0 altrimenti restituisce 1 (è una boolean)
            // NumberStyles.Any permette di accettare il numero in qualsiasi formato
            // CultureInfo.InvariantCulture forza il programma a leggere il punto come separatore decimale
            // result è il risultato ottenuto
            double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);

            return result;
        }
    }
}




