using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Windows.Interop;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPTargetProvider
    {
        private readonly double _distanceTolerance;
        private readonly int _angleTolerance;

        private List<FrameworkElement> _interactiveElementsCache = new List<FrameworkElement>();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMilliseconds(500); // Ridotto per essere più reattivi alle popup

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

        public void ForceRefreshCache(Window window)
        {
            _interactiveElementsCache.Clear();

            // 1. SCANSIONE PRIORITARIA: Cerca prima nelle Popup (tendine aperte)
            // Usiamo una logica più robusta per estrarre i figli dalle popup aperte
            var popupRoots = PresentationSource.CurrentSources.OfType<HwndSource>()
                .Select(h => h.RootVisual)
                .OfType<FrameworkElement>()
                .Where(f => f.GetType().Name.Contains("PopupRoot"));

            foreach (var root in popupRoots)
            {
                FindInteractiveElements(root, _interactiveElementsCache);
            }

            // 2. SCANSIONE SECONDARIA: Cerca nella finestra principale
            FindInteractiveElements(window, _interactiveElementsCache);

            _lastCacheUpdate = DateTime.Now;
        }

        private Rect GetElementBounds(FrameworkElement fe, Window window)
        {
            try
            {
                // Verifichiamo se l'elemento è connesso visivamente
                if (PresentationSource.FromVisual(fe) == null) return Rect.Empty;

                // Se l'elemento è nella stessa finestra, usiamo la trasformazione standard (veloce)
                if (Window.GetWindow(fe) == window)
                {
                    Point topLeft = fe.TransformToAncestor(window).Transform(new Point(0, 0));
                    return new Rect(topLeft.X, topLeft.Y, fe.ActualWidth, fe.ActualHeight);
                }
                else
                {
                    // Se l'elemento è in una Popup, usiamo il "ponte" dello schermo
                    Point screenPoint = fe.PointToScreen(new Point(0, 0));
                    Point windowPoint = window.PointFromScreen(screenPoint);
                    return new Rect(windowPoint.X, windowPoint.Y, fe.ActualWidth, fe.ActualHeight);
                }
            }
            catch { return Rect.Empty; }
        }

        public FrameworkElement GetElementAtPoint(Point windowPoint, Window window)
        {
            if (DateTime.Now - _lastCacheUpdate > _cacheDuration) ForceRefreshCache(window);

            FrameworkElement bestTarget = null;
            double minScore = double.MaxValue;
            bool isInsideAnyElement = false;

            foreach (var element in _interactiveElementsCache)
            {
                if (!element.IsVisible || element.ActualWidth == 0) continue;

                Rect bounds = GetElementBounds(element, window);
                if (bounds.IsEmpty) continue;

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
            return bestTarget;
        }

        public FrameworkElement GetNextElementInDirection(FrameworkElement currentFE, double angle, Window window)
        {
            if (currentFE == null) return null;
            if (DateTime.Now - _lastCacheUpdate > _cacheDuration) ForceRefreshCache(window);

            Rect currentBounds = GetElementBounds(currentFE, window);
            Point currentCenter = new Point(currentBounds.Left + currentBounds.Width / 2, currentBounds.Top + currentBounds.Height / 2);

            FrameworkElement bestNextFE = null;
            double minDistance = double.MaxValue;

            foreach (var target in _interactiveElementsCache)
            {
                if (target == currentFE || !target.IsVisible) continue;

                Rect targetBounds = GetElementBounds(target, window);
                if (targetBounds.IsEmpty) continue;

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
            return bestNextFE;
        }

        private double ComputeDistanceToPoint(Point pStart, Point pEnd) => Math.Sqrt(Math.Pow(pEnd.X - pStart.X, 2) + Math.Pow(pEnd.Y - pStart.Y, 2));

        private double ComputeDistanceToRect(Point p, Rect r)
        {
            double dx = Math.Max(0, Math.Max(r.Left - p.X, p.X - r.Right));
            double dy = Math.Max(0, Math.Max(r.Top - p.Y, p.Y - r.Bottom));
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void FindInteractiveElements(DependencyObject parent, List<FrameworkElement> list)
        {
            // --- GESTIONE POPUP ---
            // Le Popup non hanno figli visivi diretti nel VisualTreeHelper, 
            // scansiono esplicitamente la loro proprietà .Child
            if (parent is Popup popup)
            {
                if (popup.Child != null)
                    FindInteractiveElements(popup.Child, list);
                return;
            }

            // --- SCANSIONE STANDARD DEL VISUAL TREE ---
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe)
                {
                    // 1. Controlliamo se è un tipo di controllo interattivo che ci interessa
                    bool isTargetType = fe is Button || fe is TextBox || fe is CheckBox ||
                                       fe is RadioButton || fe is ComboBox || fe is ComboBoxItem;

                    // 2. LOGICA DI FILTRAGGIO:
                    // - Se l'elemento ha un Nome (x:Name), lo aggiungiamo (es. bottoni del form).
                    // - Se è un ComboBoxItem lo aggiungiamo SEMPRE (perché nelle popup spesso non hanno nome).
                    if (isTargetType)
                    {
                        if (!string.IsNullOrEmpty(fe.Name) || fe is ComboBoxItem)
                        {
                            list.Add(fe);
                        }
                    }
                }

                // Chiamata ricorsiva per cercare nei figli di questo elemento
                FindInteractiveElements(child, list);
            }
        }

    }

}
