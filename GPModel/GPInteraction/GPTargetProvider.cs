using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPTargetProvider
    {
        // Questo metodo riceve il punto (X,Y) relativo alla finestra e la finestra stessa
        public FrameworkElement GetElementAtPoint(Point windowPoint, Window window)
        {
            // VisualTreeHelper.HitTest è la funzione nativa di WPF che "spara un raggio" 
            // nel punto indicato e restituisce l'oggetto grafico del framework che viene colpito.
            HitTestResult target = VisualTreeHelper.HitTest(window, windowPoint);

            // Se abbiamo colpito qualcosa, cerchiamo di capire se è un elemento della UI (FrameworkElement)
            // 1. Verifichiamo prima di tutto se il test ha colpito effettivamente qualcosa
            if (target != null)
            {
                // 2. Recuperiamo l'oggetto visivo che è stato colpito
                DependencyObject visualHit = target.VisualHit;
                
                // 3. Una volta recuperato l'oggetto fissato, cerco come si chiama
                while (visualHit != null)
                {
                    // 4. Verifichiamo se questo oggetto è un FrameworkElement (un componente della UI)
                    if (visualHit is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
                    {
                        // Se lo è, lo restituiamo
                        return element;
                    }

                    // Se non ha nome guardiamo il suo genitore
                    visualHit = VisualTreeHelper.GetParent(visualHit);
                }
            }

            // Se non abbiamo colpito nulla o non è un elemento UI, ritorniamo null
            return null;
        }
    }
}
