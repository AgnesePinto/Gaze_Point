using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace Gaze_Point.GPModel.GPInteraction
{

    /// <summary>
    /// Identifies and manages interactive UI elements within the WPF visual tree.
    /// Provides spatial analysis to determine which element is under gaze or in a specific direction.
    /// </summary>
    /// <author>Agnese Pinto</author>
    

    public class GPTargetProvider
    {
        private readonly double _distanceTolerance;
        private readonly int _angleTolerance;
        private readonly TimeSpan _cacheDuration;
        private List<FrameworkElement> _interactiveElementsCache = new List<FrameworkElement>();
        private DateTime _lastCacheUpdate = DateTime.MinValue;

        public GPTargetProvider()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json", true)
                    .Build();

                _distanceTolerance = double.Parse(config["TargetProvider:DistanceTolerance"], CultureInfo.InvariantCulture);
                _angleTolerance = int.Parse(config["TargetProvider:AngleTolerance"], CultureInfo.InvariantCulture);
                int cacheMs = int.Parse(config["TargetProvider:CacheDuration"], CultureInfo.InvariantCulture);
                _cacheDuration = TimeSpan.FromMilliseconds(cacheMs);
            }
            catch
            {
                _distanceTolerance = 50.0;
                _angleTolerance = 45;
                _cacheDuration = TimeSpan.FromMilliseconds(300);
            }
        }


        /// <summary>
        /// Scans the current window and all active popups to rebuild the interactive elements cache.
        /// </summary>
        /// <param name="window">The primary application window to scan.</param>
        public void ForceRefreshCache(Window window)
        {
            _interactiveElementsCache.Clear();

            var popupRoots = PresentationSource.CurrentSources.OfType<HwndSource>()
                .Select(h => h.RootVisual)
                .OfType<FrameworkElement>()
                .Where(f => f.IsLoaded && f.IsVisible && f.GetType().Name.Contains("PopupRoot"));

            foreach (var root in popupRoots)
            {
                FindInteractiveElements(root, _interactiveElementsCache);
            }

            FindInteractiveElements(window, _interactiveElementsCache);
            _lastCacheUpdate = DateTime.Now;
        }


        /// <summary>
        /// Determines which UI element is closest to the given point.
        /// </summary>
        /// <param name="windowPoint">Gaze position relative to the window.</param>
        /// <param name="window">The reference window foe coordinate mapping.</param>
        /// <returns>The most relevant FrameworkElement or null if none are within range.</returns>
        public FrameworkElement GetElementAtPoint(Point windowPoint, Window window)
        {
            if (DateTime.Now - _lastCacheUpdate > _cacheDuration)
                ForceRefreshCache(window);

            FrameworkElement bestTarget = null;
            double minDistance = double.MaxValue;
            bool foundInside = false;

            foreach (var element in _interactiveElementsCache)
            {
                if (!element.IsVisible || PresentationSource.FromVisual(element) == null)
                    continue;

                Rect bounds = GetElementBounds(element, window);
                if (bounds.IsEmpty) continue;

                bool isInside = bounds.Contains(windowPoint);
                if (foundInside && !isInside) continue;

                double distance;
                if (isInside)
                {
                    Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
                    distance = ComputeDistanceToPoint(windowPoint, center);
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


        /// <summary>
        /// Searches for the next interactive element in specific angular direction relative to a starting element.
        /// </summary>
        /// <param name="currentFE">The starting reference element.</param>
        /// <param name="angle">The direction angle (0-360 degrees).</param>
        /// <param name="window">The reference window.</param>
        /// <returns>The closest element in that direction or null.</returns>
        public FrameworkElement GetNextElementInDirection(FrameworkElement currentFE, double angle, Window window)
        {
            if (currentFE == null) return null;

            if (DateTime.Now - _lastCacheUpdate > _cacheDuration)
                ForceRefreshCache(window);

            Rect currentBounds = GetElementBounds(currentFE, window);
            if (currentBounds.IsEmpty) return null;

            Point currentCenter = new Point(currentBounds.Left + currentBounds.Width / 2, currentBounds.Top + currentBounds.Height / 2);
            FrameworkElement bestNextFE = null;
            double minDistance = double.MaxValue;

            foreach (var target in _interactiveElementsCache)
            {
                if (target == currentFE || !target.IsVisible || target.ActualWidth == 0) continue;

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
                    double dist = ComputeDistanceToPoint(currentCenter, targetCenter);
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

        private void FindInteractiveElements(DependencyObject parent, List<FrameworkElement> list)
        {
            if (parent == null) return;

            if (parent is Popup popup && popup.IsOpen && popup.Child != null)
            {
                FindInteractiveElements(popup.Child, list);
            }

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe)
                {
                    bool isTargetType = GPInteractiveElements.InteractiveTypes.Contains(fe.GetType());

                    if (isTargetType && fe.IsVisible && fe.ActualWidth > 0)
                    {
                        if (!string.IsNullOrEmpty(fe.Name) || fe is ComboBoxItem || fe is MenuItem)
                        {
                            if (!list.Contains(fe)) list.Add(fe);
                        }
                    }
                }
                FindInteractiveElements(child, list);
            }
        }

        
        public void InvalidateCache()
        {
            _lastCacheUpdate = DateTime.MinValue; 
        }


        private Rect GetElementBounds(FrameworkElement fe, Window window)
        {
            try
            {
                if (!fe.IsLoaded || !fe.IsVisible) return Rect.Empty;

                Point screenPoint = fe.PointToScreen(new Point(0, 0));
                Point windowPoint = window.PointFromScreen(screenPoint);

                return new Rect(windowPoint.X, windowPoint.Y, fe.ActualWidth, fe.ActualHeight);
            }
            catch
            {
                return Rect.Empty;
            }
        }

        private double ComputeDistanceToPoint(Point p1, Point p2) =>
            Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));


        private double ComputeDistanceToRect(Point p, Rect r)
        {
            double dx = Math.Max(0, Math.Max(r.Left - p.X, p.X - r.Right));
            double dy = Math.Max(0, Math.Max(r.Top - p.Y, p.Y - r.Bottom));
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}