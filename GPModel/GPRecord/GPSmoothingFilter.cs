using System;

namespace Gaze_Point.GPModel.GPRecord
{
    internal class GPSmoothingFilter
    {
        // Memoria dell'ultima posizione filtrata
        private double _lastX = 0.5;
        private double _lastY = 0.5;

        // PARAMETRI DI TARATURA DEL FILTRO

        // Alfa minimo: stabilità massima quando lo sguardo è fermo (Fissazione)
        private const double AlphaMin = 0.01;       //0.05;

        // Alfa massimo: reattività massima durante gli spostamenti rapidi (Saccade)
        private const double AlphaMax = 0.3;        //0.5;

        // Sensibilità: quanto velocemente il filtro deve "aprirsi" in base alla distanza
        // Un valore più alto rende il cursore più nervoso ma più pronto
        // è il coefficiente angolare della nostra retta, ci dice quanto deve essere rapido lo spostamento tra Alphamax e min
        private const double Sensitivity = 3.0;          //5.0;

        public GPData AdaptiveSmoothing(GPData data)
        {
            if (data == null) return null;

            // 1. CALCOLO DELLA DISTANZA (Euclidea) tra il nuovo dato e l'ultima posizione filtrata
            double dx = data.BPOGX - _lastX;
            double dy = data.BPOGY - _lastY;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            // 2. CALCOLO DINAMICO DI ALFA
            // Più distanza c'è, più alfa aumenta (entro i limiti stabiliti)
            double dynamicAlpha = AlphaMin + (distance * Sensitivity);

            // Controllo di Alfa: deve stare nel range [AlphaMin, AlphaMax]
            double alpha = Math.Max(AlphaMin, Math.Min(AlphaMax, dynamicAlpha));

            // 3. FILTRO BUTTERWORTH DEL PRIMO ORDINE
            // NuovaPos = (Alfa * NuovoDato) + ((1 - Alfa) * VecchiaPos)
            double smoothX = (alpha * data.BPOGX) + ((1 - alpha) * _lastX);
            double smoothY = (alpha * data.BPOGY) + ((1 - alpha) * _lastY);

            // 4. AGGIORNAMENTO MEMORIA E DATI
            _lastX = smoothX;
            _lastY = smoothY;

            data.BPOGX = smoothX;
            data.BPOGY = smoothY;

            return data;
        }
    }
}
