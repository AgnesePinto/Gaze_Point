using System;
using System.Windows;

namespace Gaze_Point.GPModel.GPRecord
{
    public class GPConverter
    {
        /// <summary>
        /// Converte le coordinate normalizzate (0-1) in Pixel Fisici (interi).
        /// Necessario per le API di sistema come SetCursorPos.
        /// </summary>
        public static (int X, int Y) ToPhysicalScreenPoint(GPData data)
        {
            double gpX = data.BPOGX;  
            double gpY = data.BPOGY;

            // Inizializza i fattori di scala ad 1, cioè a 100% su windows
            double scaleX = 1.0;
            double scaleY = 1.0;

            //Controlla se esiste una finestra attiva per calcolare lo scaling DPI
            if (Application.Current.MainWindow != null)
            {
                // Ottiene la sorgene visiva, cioè il legame tra software e hardware visivo
                var source = PresentationSource.FromVisual(Application.Current.MainWindow);

                // Se il sistema di randering è attivo, vengono rcuperati gli zoom (M11 e M22)
                if (source?.CompositionTarget != null)
                {
                    scaleX = source.CompositionTarget.TransformToDevice.M11;
                    scaleY = source.CompositionTarget.TransformToDevice.M22;
                }
            }

            // Calcolo: (Dato normalizzato * Larghezza Logica) * Scala = Pixel Fisici
            // 1. (gpX * SystemParameters.PrimaryScreenWidth): Trova la posizione logica (es. 960 su 1920)
            // 2. (* scaleX): Moltiplica per lo zoom per trovare il pixel fisico (es. 960 * 1.25 = 1200)
            // 3. (int): Converte il decimale in numero intero (il cursore non sta tra due pixel)
            int physX = (int)(gpX * SystemParameters.PrimaryScreenWidth * scaleX);
            int physY = (int)(gpY * SystemParameters.PrimaryScreenHeight * scaleY);

            return (physX, physY);
        }

        public static Point ToWindowPoint(Point physicalPoint, Window window)
        {
            // Prende il punto fisico dello schermo (pixel reali) 
            // e lo traduce in un punto logico interno alla finestra.
            // Gestisce da solo: posizione finestra e zoom di Windows (125%, 150%, ecc.)
            return window.PointFromScreen(physicalPoint);
        }

        public static (double X, double Y) ToLogicalScreenPoint(GPData data)
        {
            // Calcolo: Dato normalizzato (0-1) * Larghezza Schermo Logica
            // SystemParameters.PrimaryScreenWidth restituisce già il valore "pulito" dallo zoom DPI
            double logX = data.BPOGX * SystemParameters.PrimaryScreenWidth;
            double logY = data.BPOGY * SystemParameters.PrimaryScreenHeight;

            return (logX, logY);
        }
    }
}

