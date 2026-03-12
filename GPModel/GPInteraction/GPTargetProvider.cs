//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Media;
//using System.Windows;
//using System.Windows.Controls;

//namespace Gaze_Point.GPModel.GPInteraction
//{
//    public class GPTargetProvider
//    {
//        // Questo metodo riceve il punto (X,Y) relativo alla finestra e la finestra stessa
//        public FrameworkElement GetElementAtPoint(Point windowPoint, Window window)
//        {
//            // VisualTreeHelper.HitTest è la funzione nativa di WPF che "spara un raggio" 
//            // nel punto indicato e restituisce l'oggetto grafico del framework che viene colpito.
//            HitTestResult target = VisualTreeHelper.HitTest(window, windowPoint);

//            // Se abbiamo colpito qualcosa, cerchiamo di capire se è un elemento della UI (FrameworkElement)
//            // 1. Verifichiamo prima di tutto se il test ha colpito effettivamente qualcosa
//            if (target != null)
//            {
//                DependencyObject obj = target.VisualHit;

//                while (obj != null)
//                {
//                    // CERCHIAMO SIA BOTTONI CHE TEXTBOX
//                    // Se l'oggetto è un FrameworkElement e ha un nome, lo consideriamo un bersaglio
//                    if (obj is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
//                    {
//                        // Filtriamo solo i tipi che ci interessano: Button e TextBox
//                        if (fe is Button || fe is TextBox)
//                        {
//                            return fe;
//                        }
//                    }

//                    // Risaliamo verso il genitore se non abbiamo trovato nulla
//                    obj = VisualTreeHelper.GetParent(obj);
//                }
//            }
//            return null;
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPTargetProvider
    {
        private readonly double _tolerance;

        public GPTargetProvider()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                _tolerance = double.Parse(config["Interaction:TolerancePixels"] ?? "50.0");
            }
            catch
            {
                _tolerance = 50.0; // Fallback
            }
        }

        public FrameworkElement GetElementAtPoint(Point windowPoint, Window window)
        {
            FrameworkElement bestTarget = null;
            double minDistance = double.MaxValue;

            // 1. Cerchiamo tutti i potenziali bersagli nella finestra
            var interactiveElements = new List<FrameworkElement>();
            FindInteractiveElements(window, interactiveElements);

            foreach (var element in interactiveElements)
            {
                if (!element.IsVisible) continue;

                // 2. Troviamo il centro dell'elemento rispetto alla finestra
                // Trasformiamo il punto centrale (ActualWidth/2) in coordinate Window
                // Trasforma la posizione locale dell'elemento grafico in una posizione relativa alla finestra
                Point elementCenter = element.TransformToAncestor(window)
                                             .Transform(new Point(element.ActualWidth / 2, element.ActualHeight / 2));

                // 3. Calcolo distanza Euclidea
                double dx = windowPoint.X - elementCenter.X;
                double dy = windowPoint.Y - elementCenter.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                // 4. Selezione del "vincitore" entro la soglia (Magnetismo)
                if (distance < _tolerance && distance < minDistance)
                {
                    minDistance = distance;
                    bestTarget = element;
                }
            }
            return bestTarget;
        }

        private void FindInteractiveElements(DependencyObject parent, List<FrameworkElement> list)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
                {
                    // Filtriamo solo ciò che ci interessa cliccare/scrivere
                    if (fe is Button || fe is TextBox || fe is CheckBox)
                        list.Add(fe);
                }
                FindInteractiveElements(child, list); // Ricorsione
            }
        }
    }
}

