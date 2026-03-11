using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

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
                DependencyObject obj = target.VisualHit;

                while (obj != null)
                {
                    // CERCHIAMO SIA BOTTONI CHE TEXTBOX
                    // Se l'oggetto è un FrameworkElement e ha un nome, lo consideriamo un bersaglio
                    if (obj is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
                    {
                        // Filtriamo solo i tipi che ci interessano: Button e TextBox
                        if (fe is Button || fe is TextBox)
                        {
                            return fe;
                        }
                    }

                    // Risaliamo verso il genitore se non abbiamo trovato nulla
                    obj = VisualTreeHelper.GetParent(obj);
                }
            }
            return null;
        }
    }
}
