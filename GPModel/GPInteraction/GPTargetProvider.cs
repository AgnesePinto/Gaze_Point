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
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMilliseconds(300); // Più veloce per interazioni UI

        public GPTargetProvider()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json", true)
                    .Build();

                _distanceTolerance = double.TryParse(config["Interaction:DistanceTolerance"], out var dt) ? dt : 50.0;
                _angleTolerance = int.TryParse(config["Interaction:AngleTolerance"], out var at) ? at : 45;
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

            // 1. SCANSIONE POPUP (Tendine aperte, ContextMenu, ecc.)
            // PresentationSource.CurrentSources permette di trovare le finestre "nascoste" dei popup
            var popupRoots = PresentationSource.CurrentSources.OfType<HwndSource>()
                .Select(h => h.RootVisual)
                .OfType<FrameworkElement>()
                .Where(f => f.GetType().Name.Contains("PopupRoot"));

            foreach (var root in popupRoots)
            {
                FindInteractiveElements(root, _interactiveElementsCache);
            }

            // 2. SCANSIONE FINESTRA PRINCIPALE
            FindInteractiveElements(window, _interactiveElementsCache);

            _lastCacheUpdate = DateTime.Now;
        }

        private void FindInteractiveElements(DependencyObject parent, List<FrameworkElement> list)
        {
            if (parent == null) return;

            // Logica speciale per l'oggetto Popup (ponte tra Visual Trees diversi)
            if (parent is Popup popup && popup.IsOpen && popup.Child != null)
            {
                FindInteractiveElements(popup.Child, list);
            }

            // Scansione figli tramite VisualTreeHelper
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe)
                {
                    // Definiamo cosa è "interattivo"
                    bool isTargetType = fe is Button || fe is TextBox || fe is CheckBox ||
                                       fe is RadioButton || fe is ComboBox || fe is ComboBoxItem ||
                                       fe is MenuItem;

                    if (isTargetType && fe.IsVisible && fe.ActualWidth > 0)
                    {
                        // Aggiungiamo se ha un nome o se è un elemento di una lista (che spesso non ha nome)
                        if (!string.IsNullOrEmpty(fe.Name) || fe is ComboBoxItem || fe is MenuItem)
                        {
                            if (!list.Contains(fe)) list.Add(fe);
                        }
                    }
                }
                // Ricorsione
                FindInteractiveElements(child, list);
            }
        }

        private Rect GetElementBounds(FrameworkElement fe, Window window)
        {
            try
            {
                if (!fe.IsLoaded || !fe.IsVisible) return Rect.Empty;

                // Calcolo coordinate assolute rispetto allo schermo
                Point screenPoint = fe.PointToScreen(new Point(0, 0));

                // Conversione coordinate schermo -> coordinate relative della finestra principale
                Point windowPoint = window.PointFromScreen(screenPoint);

                return new Rect(windowPoint.X, windowPoint.Y, fe.ActualWidth, fe.ActualHeight);
            }
            catch { return Rect.Empty; }
        }

        public FrameworkElement GetElementAtPoint(Point windowPoint, Window window)
        {
            if (DateTime.Now - _lastCacheUpdate > _cacheDuration) ForceRefreshCache(window);

            FrameworkElement bestTarget = null;
            double minDistance = double.MaxValue;
            bool foundInside = false;

            foreach (var element in _interactiveElementsCache)
            {
                Rect bounds = GetElementBounds(element, window);
                if (bounds.IsEmpty) continue;

                bool isInside = bounds.Contains(windowPoint);

                // Se siamo già "dentro" un elemento, ignoriamo quelli "fuori"
                if (foundInside && !isInside) continue;

                double distance;
                if (isInside)
                {
                    // Se siamo dentro, la distanza è dal centro (per precisione gaze)
                    Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
                    distance = ComputeDistanceToPoint(windowPoint, center);

                    // Priorità assoluta ai ComboBoxItem se siamo dentro (stanno sopra tutto)
                    if (element is ComboBoxItem) distance -= 1000;

                    if (!foundInside)
                    {
                        foundInside = true;
                        minDistance = distance;
                        bestTarget = element;
                    }
                }
                else
                {
                    distance = ComputeDistanceToRect(windowPoint, bounds);
                }

                if (distance < minDistance && (isInside || distance < _distanceTolerance))
                {
                    minDistance = distance;
                    bestTarget = element;
                }
            }
            return bestTarget;
        }

        // Metodi di utilità geometrica
        private double ComputeDistanceToPoint(Point p1, Point p2)
            => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

        private double ComputeDistanceToRect(Point p, Rect r)
        {
            double dx = Math.Max(0, Math.Max(r.Left - p.X, p.X - r.Right));
            double dy = Math.Max(0, Math.Max(r.Top - p.Y, p.Y - r.Bottom));
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public FrameworkElement GetNextElementInDirection(FrameworkElement currentFE, double angle, Window window)
        {
            if (currentFE == null) return null;

            // Aggiorna la cache se necessario
            if (DateTime.Now - _lastCacheUpdate > _cacheDuration) ForceRefreshCache(window);

            // Ottieni i confini dell'elemento corrente (gestisce correttamente se è in un popup)
            Rect currentBounds = GetElementBounds(currentFE, window);
            if (currentBounds.IsEmpty) return null;

            Point currentCenter = new Point(currentBounds.Left + currentBounds.Width / 2, currentBounds.Top + currentBounds.Height / 2);

            FrameworkElement bestNextFE = null;
            double minDistance = double.MaxValue;

            foreach (var target in _interactiveElementsCache)
            {
                // Salta l'elemento corrente e quelli non visibili
                if (target == currentFE || !target.IsVisible || target.ActualWidth == 0) continue;

                Rect targetBounds = GetElementBounds(target, window);
                if (targetBounds.IsEmpty) continue;

                Point targetCenter = new Point(targetBounds.Left + targetBounds.Width / 2, targetBounds.Top + targetBounds.Height / 2);

                // Calcolo dell'angolo tra il centro attuale e il target
                double dx = targetCenter.X - currentCenter.X;
                double dy = targetCenter.Y - currentCenter.Y;
                double targetAngle = Math.Atan2(dy, dx) * (180 / Math.PI);

                // Normalizza l'angolo in [0, 360]
                if (targetAngle < 0) targetAngle += 360;

                // Calcolo differenza angolare minima
                double diff = Math.Abs(targetAngle - angle);
                if (diff > 180) diff = 360 - diff;

                // Se l'elemento è nella direzione desiderata (entro la tolleranza)
                if (diff < _angleTolerance)
                {
                    double dist = ComputeDistanceToPoint(currentCenter, targetCenter);

                    // Priorità agli elementi nel popup se stiamo navigando
                    if (target is ComboBoxItem) dist -= 500;

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

