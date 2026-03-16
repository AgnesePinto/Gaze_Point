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
        private readonly double _distanceTolerance;
        private readonly int _angleTolerance;

        public GPTargetProvider()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                _distanceTolerance = double.Parse(config["Interaction:DistanceTolerance"]);
                _angleTolerance = int.Parse(config["Interaction:AngleTolerance"]);
            }
            catch
            {
                // Fallback
                _distanceTolerance = 50.0; 
                _angleTolerance = 45;   
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
                    if (distance < _distanceTolerance && distance < minDistance)
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

        public FrameworkElement GetNextElementInDirection(FrameworkElement currentFE, double angle, Window window)
        {
            if (currentFE == null) return null;

            // Centro dell'elemento attuale 
            Point currentCenter = currentFE.TransformToAncestor(window).Transform(FindCenterPoint(currentFE));

            FrameworkElement bestNextFE = null;
            double minDistance = double.MaxValue;

            var candidates = new List<FrameworkElement>();
            FindInteractiveElements(window, candidates);

            foreach (var target in candidates)
            {
                if (target == currentFE || !target.IsVisible) continue;

                // Centro del nuovo target
                Point targetCenter = currentFE.TransformToAncestor(window).Transform(FindCenterPoint(currentFE));

                // Calcolo vettore verso il candidato
                double dx = targetCenter.X - currentCenter.X;
                double dy = targetCenter.Y - currentCenter.Y;
                double dist = ComputeDistanceToPoint(currentCenter, targetCenter);

                // Calcolo angolo verso il candidato
                double targetAngle = Math.Atan2(dy, dx) * (180 / Math.PI);

                if (targetAngle < 0) targetAngle += 360;

                // Verifichiamo se il candidato è nel cono di tolleranza
                double diff = Math.Abs(targetAngle - angle);
                if (diff > 180) diff = 360 - diff;

                // L'elemento è nel "cono" visivo della direzione indicata
                if (diff < _angleTolerance) 
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

        private double ComputeDistanceToPoint (Point pStart, Point pEnd)
        {
            double dx = pEnd.X - pStart.X;
            double dy = pEnd.Y - pStart.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return dist;
        }

        private Point FindCenterPoint (FrameworkElement fe)
        {
            double cx = fe.ActualWidth / 2;
            double cy = fe.ActualHeight / 2;
            Point pCenter = new Point(cx, cy);
            return pCenter;
        }

        private double ComputeDistanceToRect(Point p, Rect r)
        {
            // Calcolo distanza sui due assi: se il punto è "dentro" l'asse, il valore è 0
            double dx = Math.Max(0, Math.Max(r.Left - p.X, p.X - r.Right));
            double dy = Math.Max(0, Math.Max(r.Top - p.Y, p.Y - r.Bottom));
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return dist;
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

    }
}

