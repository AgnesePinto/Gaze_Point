using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Gaze_Point.GPModel.GPRecord
{
    internal class GPValidationFilter
    {
        private double _lastValidX = 0.5;
        private double _lastValidY = 0.5;

        // Variabili per il Blanking Period
        private bool _wasValid = true;
        private int _recoverySamples = 0;

        private readonly double MinRange;
        private readonly double MaxRange;

        public GPValidationFilter()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                MinRange = double.Parse(config["Validation:MinRange"]);
                MaxRange = double.Parse(config["Validation:MaxRange"]);
            }
            catch
            {
                MinRange = 0.01;
                MaxRange = 0.99;
            }
        }

        public GPData ValidationFilter(GPData rawData)
        {
            if (rawData == null) return null;

            bool currentValid = IsValid(rawData);

            // RILEVAZIONE RIAPERTURA OCCHI (Transizione da Invalido a Valido)
            if (currentValid && !_wasValid)
            {
                _recoverySamples = 3; // Imposta il Blanking Period (circa 20ms a 150Hz)
            }
            _wasValid = currentValid;

            // LOGICA DI FILTRAGGIO E BLANKING
            if (!currentValid || _recoverySamples > 0)
            {
                // Se l'occhio è chiuso OPPURE siamo nel periodo di stabilizzazione post-blink:
                // Sovrascriviamo con l'ultima posizione stabile
                rawData.BPOGX = _lastValidX;
                rawData.BPOGY = _lastValidY;

                // Forziamo BPOGV a 1 solo internamente per non interrompere il flusso dei filtri successivi
                rawData.BPOGV = 1;

                // Scalo il contatore per i cicli successivi
                if (_recoverySamples > 0) _recoverySamples--;
            }
            else
            {
                // Se il dato è reale e stabilizzato, aggiorniamo la memoria
                _lastValidX = rawData.BPOGX;
                _lastValidY = rawData.BPOGY;
            }

            // 2. RETTANGOLO DI DENOISE (Clamping)
            rawData.BPOGX = Math.Max(MinRange, Math.Min(MaxRange, rawData.BPOGX));
            rawData.BPOGY = Math.Max(MinRange, Math.Min(MaxRange, rawData.BPOGY));

            return rawData;
        }

        public bool IsValid(GPData rawData)
        {
            return rawData.BPOGV == 1;
        }
    }
}


