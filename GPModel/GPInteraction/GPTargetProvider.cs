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

        private List<FrameworkElement> _interactiveElementsCache = new List<FrameworkElement>();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(1);

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
                _distanceTolerance = 50.0;
                _angleTolerance = 45;
            }
        }

        // Metodo per forzare l'aggiornamento (usato al cambio finestra)
        public void ForceRefreshCache(Window window)
        {
            _interactiveElementsCache.Clear();
            FindInteractiveElements(window, _interactiveElementsCache);
            _lastCacheUpdate = DateTime.Now;
        }

        public FrameworkElement GetElementAtPoint(Point windowPoint, Window window)
        {
            // Controllo temporale per aggiornamento automatico (Resilienza)
            if (DateTime.Now - _lastCacheUpdate > _cacheDuration)
            {
                ForceRefreshCache(window);
            }

            FrameworkElement bestTarget = null;
            double minScore = double.MaxValue;
            bool isInsideAnyElement = false;

            foreach (var element in _interactiveElementsCache)
            {
                if (!element.IsVisible) continue;

                try
                {
                    Point topLeft = element.TransformToAncestor(window).Transform(new Point(0, 0));
                    Rect bounds = new Rect(topLeft.X, topLeft.Y, element.ActualWidth, element.ActualHeight);

                    double distanceToEdge = ComputeDistanceToRect(windowPoint, bounds);
                    bool isInside = (distanceToEdge == 0);

                    if (isInsideAnyElement && !isInside) continue;

                    double currentScore;
                    if (isInside)
                    {
                        Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
                        currentScore = ComputeDistanceToPoint(windowPoint, center);

                        if (!isInsideAnyElement)
                        {
                            isInsideAnyElement = true;
                            minScore = currentScore;
                            bestTarget = element;
                        }
                    }
                    else
                    {
                        currentScore = distanceToEdge;
                    }

                    if (currentScore < minScore && (isInside || currentScore < _distanceTolerance))
                    {
                        minScore = currentScore;
                        bestTarget = element;
                    }
                }
                catch { continue; }
            }
            return bestTarget;
        }

        public FrameworkElement GetNextElementInDirection(FrameworkElement currentFE, double angle, Window window)
        {
            if (currentFE == null) return null;

            // Assicuriamoci che la cache sia valida anche per la navigazione direzionale
            if (DateTime.Now - _lastCacheUpdate > _cacheDuration) ForceRefreshCache(window);

            Point currentCenter = currentFE.TransformToAncestor(window).Transform(FindCenterPoint(currentFE));
            FrameworkElement bestNextFE = null;
            double minDistance = double.MaxValue;

            foreach (var target in _interactiveElementsCache)
            {
                if (target == currentFE || !target.IsVisible) continue;

                try
                {
                    Point topLeft = target.TransformToAncestor(window).Transform(new Point(0, 0));
                    Rect targetBounds = new Rect(topLeft.X, topLeft.Y, target.ActualWidth, target.ActualHeight);
                    Point targetCenter = new Point(targetBounds.Left + targetBounds.Width / 2, targetBounds.Top + targetBounds.Height / 2);

                    double dx = targetCenter.X - currentCenter.X;
                    double dy = targetCenter.Y - currentCenter.Y;
                    double targetAngle = Math.Atan2(dy, dx) * (180 / Math.PI);
                    if (targetAngle < 0) targetAngle += 360;

                    double diff = Math.Abs(targetAngle - angle);
                    if (diff > 180) diff = 360 - diff;

                    if (diff < _angleTolerance)
                    {
                        double dist = ComputeDistanceToRect(currentCenter, targetBounds);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            bestNextFE = target;
                        }
                    }
                }
                catch { continue; }
            }
            return bestNextFE;
        }

        private double ComputeDistanceToPoint(Point pStart, Point pEnd) => Math.Sqrt(Math.Pow(pEnd.X - pStart.X, 2) + Math.Pow(pEnd.Y - pStart.Y, 2));
        private Point FindCenterPoint(FrameworkElement fe) => new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
        private double ComputeDistanceToRect(Point p, Rect r)
        {
            double dx = Math.Max(0, Math.Max(r.Left - p.X, p.X - r.Right));
            double dy = Math.Max(0, Math.Max(r.Top - p.Y, p.Y - r.Bottom));
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
                    if (fe is Button || fe is TextBox || fe is CheckBox || fe is RadioButton)
                        list.Add(fe);
                }
                FindInteractiveElements(child, list);
            }
        }
    }
}
