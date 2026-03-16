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

            var interactiveElements = new List<FrameworkElement>();
            FindInteractiveElements(window, interactiveElements);

            foreach (var element in interactiveElements)
            {
                if (!element.IsVisible) continue;

                try
                {
                    // 1. Otteniamo i confini (Rect) dell'elemento rispetto alla finestra
                    // Trasformiamo il punto (0,0) dell'elemento in coordinate finestra per trovare l'angolo in alto a sinistra
                    Point topLeft = element.TransformToAncestor(window).Transform(new Point(0, 0));
                    Rect bounds = new Rect(topLeft.X, topLeft.Y, element.ActualWidth, element.ActualHeight);

                    // 2. Calcoliamo la distanza minima tra il punto dello sguardo e il rettangolo dell'oggetto
                    double distance = ComputeDistanceToRect(windowPoint, bounds);

                    // 3. Se lo sguardo è sopra l'oggetto (distanza 0) o entro la tolleranza dal bordo
                    if (distance < _tolerance && distance < minDistance)
                    {
                        minDistance = distance;
                        bestTarget = element;
                    }
                }
                catch
                {
                    // Gestisce casi in cui l'elemento non è ancora renderizzato o staccato dal visual tree
                    continue;
                }
            }
            return bestTarget;
        }

        /// <summary>
        /// Calcola la distanza minima tra un punto e i bordi di un rettangolo.
        /// Restituisce 0 se il punto è all'interno del rettangolo.
        /// </summary>
        private double ComputeDistanceToRect(Point p, Rect r)
        {
            // Calcolo distanza sui due assi: se il punto è "dentro" l'asse, il valore è 0
            double dx = Math.Max(0, Math.Max(r.Left - p.X, p.X - r.Right));
            double dy = Math.Max(0, Math.Max(r.Top - p.Y, p.Y - r.Bottom));

            // Pitagora per la distanza euclidea dai bordi
            return Math.Sqrt(dx * dx + dy * dy);
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
                    if (fe is Button || fe is TextBox || fe is CheckBox || fe is RadioButton)
                        list.Add(fe);
                }
                FindInteractiveElements(child, list); // Ricorsione
            }
        }

        public FrameworkElement GetNextElementInDirection(FrameworkElement currentFE, double angle, Window window)
        {
            if (currentFE == null) return null;

            // Centro dell'elemento attuale
            Point currentCenter = currentFE.TransformToAncestor(window)
                                         .Transform(new Point(currentFE.ActualWidth / 2, currentFE.ActualHeight / 2));

            FrameworkElement bestNextFE = null;
            double minDistance = double.MaxValue;

            var candidates = new List<FrameworkElement>();
            FindInteractiveElements(window, candidates);

            foreach (var target in candidates)
            {
                if (target == currentFE || !target.IsVisible) continue;

                // Centro del nuovo target
                Point targetCenter = target.TransformToAncestor(window)
                                           .Transform(new Point(target.ActualWidth / 2, target.ActualHeight / 2));

                // Calcolo vettore verso il candidato
                double dx = targetCenter.X - currentCenter.X;
                double dy = targetCenter.Y - currentCenter.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                // Calcolo angolo verso il candidato
                double targetAngle = Math.Atan2(dy, dx) * (180 / Math.PI);
                if (targetAngle < 0) targetAngle += 360;

                // Verifichiamo se il candidato è nella direzione desiderata (con un cono di tolleranza di 45 gradi)
                double diff = Math.Abs(targetAngle - angle);
                if (diff > 180) diff = 360 - diff;

                if (diff < 45) // L'elemento è nel "cono" visivo della direzione indicata
                {
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestNextFE = target;
                    }
                }
            }
            return bestNextFE;
        }

    }
}

