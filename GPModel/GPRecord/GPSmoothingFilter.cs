using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Gaze_Point.GPModel.GPRecord
{
    internal class GPSmoothingFilter
    {
        // Memoria dell'ultima posizione filtrata
        private double _lastX = 0.5;
        private double _lastY = 0.5;

        // PARAMETRI DI TARATURA DEL FILTRO

        // Alfa minimo: stabilità massima quando lo sguardo è fermo (Fissazione)
        private readonly double AlphaMin;       

        // Alfa massimo: reattività massima durante gli spostamenti rapidi (Saccade)
        private readonly double AlphaMax;       

        // Sensibilità: quanto velocemente il filtro deve "aprirsi" in base alla distanza
        // Un valore più alto rende il cursore più nervoso ma più pronto
        // è il coefficiente angolare della nostra retta, ci dice quanto deve essere rapido lo spostamento tra Alphamax e min
        private readonly double Sensitivity;

        // Distanza accettabile per eliminare il tremolio
        private readonly double distanceThreshold;

        public GPSmoothingFilter()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                AlphaMin = double.Parse(config["Smoothing:AlphaMin"]);
                AlphaMax = double.Parse(config["Smoothing:AlphaMax"]);
                Sensitivity = double.Parse(config["Smoothing:Sensitivity"]);
                distanceThreshold = double.Parse(config["Smoothing:DistanceThreshold"]);
            }
            catch
            {
                // Fallback in caso di errore caricamento file
                AlphaMin = 0.01;
                AlphaMax = 0.3;
                Sensitivity = 3.0;
            }
        }

        public GPData AdaptiveSmoothing(GPData data)
        {
            if (data == null) return null;

            // 1. CALCOLO DELLA DISTANZA (Euclidea) tra il nuovo dato e l'ultima posizione filtrata
            double dx = data.BPOGX - _lastX;
            double dy = data.BPOGY - _lastY;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if(distance > distanceThreshold)
            {
                // 2. CALCOLO DINAMICO DI ALFA
                // Più distanza c'è, più alfa aumenta (entro i limiti stabiliti)
                double dynamicAlpha = AlphaMin + (distance * Sensitivity);

                // Controllo di Alfa: deve stare nel range [AlphaMin, AlphaMax]
                if (dynamicAlpha < AlphaMin)
                {
                    dynamicAlpha = AlphaMin; // Se è troppo piccolo, alzalo al minimo
                }
                else if (dynamicAlpha > AlphaMax)
                {
                    dynamicAlpha = AlphaMax; // Se è troppo grande, abbassalo al massimo
                }

                // 3. FILTRO BUTTERWORTH DEL PRIMO ORDINE
                // NuovaPos = (Alfa * NuovoDato) + ((1 - Alfa) * VecchiaPos)
                double smoothX = (dynamicAlpha * data.BPOGX) + ((1 - dynamicAlpha) * _lastX);
                double smoothY = (dynamicAlpha * data.BPOGY) + ((1 - dynamicAlpha) * _lastY);

                // 4. AGGIORNAMENTO MEMORIA E DATI
                _lastX = smoothX;
                _lastY = smoothY;

                data.BPOGX = smoothX;
                data.BPOGY = smoothY;

                return data;
            } else
            {
                data.BPOGX = _lastX;
                data.BPOGY = _lastY; 
                return data;
            }
        }
    }
}
