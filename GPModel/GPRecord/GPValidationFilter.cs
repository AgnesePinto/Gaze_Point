using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaze_Point.GPModel.GPRecord
{
    internal class GPValidationFilter
    {
        // Ultima coordinata valida per gestire i blink
        private double _lastValidX = 0.5;
        private double _lastValidY = 0.5;

        // Parametri del rettangolo di denoise (range operativo sul display)
        private const double MinRange = 0.00;
        private const double MaxRange = 1.00;

        public GPData ValidationFilter (GPData rawData)
        {
            if (rawData == null) return null;

            // 1. GESTIONE BLINK (Validazione)
            if (!IsValid(rawData))
            {
                // Se l'occhio è perso (blink), sovrascriviamo con l'ultima posizione buona
                // In questo modo il cursore resta "congelato" e non schizza a (0,0)
                rawData.BPOGX = _lastValidX;
                rawData.BPOGY = _lastValidY;

                // Forzo BPOGV ad 1 così il resto del programma pensa che sia una dato utilizzabile
                rawData.BPOGV = 1;
            }
            else
            {
                // Se il dato è valido, aggiorniamo l'ultima posizione nota
                _lastValidX = rawData.BPOGX;
                _lastValidY = rawData.BPOGY;
            }

            // 2. RETTAGOLO DI DENOISE (Clamping Operativo)
            // Applichiamo la teoria: restringiamo il campo per eliminare distorsioni ai bordi
            rawData.BPOGX = Math.Max(MinRange, Math.Min(MaxRange, rawData.BPOGX));
            rawData.BPOGY = Math.Max(MinRange, Math.Min(MaxRange, rawData.BPOGY));

            return rawData;
        }

        // Controllo per dato affidabile
        public bool IsValid (GPData rawData)
        {
                if (rawData.BPOGV == 1)
                {
                    return true;
                }
                return false;
        }
    }
}


