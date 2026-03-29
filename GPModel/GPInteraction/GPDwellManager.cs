using System;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Gaze_Point.GPModel.GPInteraction
{

    /// <summary>
    /// Monitors gaze duration on UI elements to detect stable fixation (dwell time).
    /// Triggers an event when the gaze remains on an element longer than the configured threshold.
    /// </summary>
    /// <author>Agnese Pinto</author>
    

    public class GPDwellManager
    {
        private readonly TimeSpan _dwellTime;

        private FrameworkElement _currentElement;
        private DateTime _focusStartTime;

        public event Action<FrameworkElement> OnElementFocused;

        public GPDwellManager()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                int ms = int.Parse(config["DwellTimeManager:DwellTimeMs"]);
                _dwellTime = TimeSpan.FromMilliseconds(ms);
            }
            catch
            {
                // Fallback
                _dwellTime = TimeSpan.FromMilliseconds(30);
            }
        }


        /// <summary>
        /// Evaluates the current element under gaze to track fixation progress.
        /// </summary>
        /// <param name="elementUnderGaze">The UI element currently targeted by the gaze provider.</param>
        public void Update(FrameworkElement elementUnderGaze)
        {
            if (elementUnderGaze == null)
            {
                _currentElement = null;
                return;
            }

            if (elementUnderGaze != _currentElement)
            {
                _currentElement = elementUnderGaze;
                _focusStartTime = DateTime.Now; 
                return;
            }

            var fixationTime = DateTime.Now - _focusStartTime;
            if (fixationTime >= _dwellTime)
            {
                OnElementFocused?.Invoke(_currentElement);

                _focusStartTime = DateTime.MaxValue;
            }
        }


        /// <summary>
        /// Resets the internal state, clearing the current elements and timers.
        /// </summary>
        public void Clear()
        {
            _currentElement = null;
            _focusStartTime = DateTime.MinValue;
        }


        public void ResetDwellTimer()
        {
            _focusStartTime = DateTime.Now;
        }
    }
}

