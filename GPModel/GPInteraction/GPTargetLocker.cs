using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Gaze_Point.GPModel.GPInteraction
{
    /// <summary>
    /// Temporarly locks interactive updates to collect a series of gaze samples.
    /// This period allows for movement analysis without immediate re-triggering of UI elements.
    /// </summary>
    /// <author>Agnese Pinto</author>
    public class GPTargetLocker
    {
        private readonly DispatcherTimer _timer;
        private readonly double _lockTime;
        private bool _isLocked;
        private readonly List<Point> _collectedPoints = new List<Point>();

        public bool IsLocked => _isLocked;
        public event Action<List<Point>> OnLockExpired;

        public GPTargetLocker()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                _lockTime = double.Parse(config["TargetLocker:LockTime"], CultureInfo.InvariantCulture);
            }
            catch
            {
                // Fallback 
                _lockTime = 1500.0;
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_lockTime);
            _timer.Tick += (s, e) => ReleaseLock();
        }

        /// <summary>
        /// Activates the lock, clearing previous data and starting countdown.
        /// </summary>
        public void Activate()
        {
            _collectedPoints.Clear();
            _isLocked = true;
            _timer.Stop();
            _timer.Start();
        }

        /// <summary>
        /// Processes and stores a gaze point if the lock is active and the data is valid.
        /// </summary>
        /// <param name="x">The normalized X coordinate.</param>
        /// <param name="y">The normalized Y coordinate.</param>
        /// <param name="bpogv">The validity flag from the eye-tracker (1 = valid).</param>
        public void ProcessPoint(double x, double y, int bpogv)
        {
            if (!_isLocked) return;

            if (bpogv == 1)
            {
                _collectedPoints.Add(new Point(x, y));
            }
        }

        private void ReleaseLock()
        {
            _timer.Stop();
            _isLocked = false;
            OnLockExpired?.Invoke(new List<Point>(_collectedPoints));
            _collectedPoints.Clear();
        }
    }
}






//using System;
//using System.Collections.Generic;
//using System.Windows;
//using System.Windows.Threading;
//using Microsoft.Extensions.Configuration;
//using System.IO;

//namespace Gaze_Point.GPModel.GPInteraction
//{

//    /// <summary>
//    /// Temporarly locks interactive updates to collect a series of gaze samples.
//    /// This period allows for movement analysis without immediate re-triggering of UI elements.
//    /// </summary>
//    /// <author>Agnese Pinto</author>


//    public class GPTargetLocker
//    {
//        private readonly DispatcherTimer _timer;
//        private readonly double _lockTime;

//        private bool _isLocked;
//        private readonly List<Point> _collectedPoints = new List<Point>();

//        public bool IsLocked => _isLocked;

//        public event Action<List<Point>> OnLockExpired;

//        public GPTargetLocker()
//        {
//            try
//            {
//                var config = new ConfigurationBuilder()
//                    .SetBasePath(Directory.GetCurrentDirectory())
//                    .AddJsonFile("AppSettings/DataSettings.json")
//                    .Build();

//                _lockTime = double.Parse(config["TargetLocker:LockTime"]);
//            }
//            catch
//            {
//                // Fallback
//                _lockTime = 1500.0;
//            }

//            _timer = new DispatcherTimer();
//            _timer.Interval = TimeSpan.FromMilliseconds(_lockTime);
//            _timer.Tick += (s, e) => ReleaseLock();
//        }


//        /// <summary>
//        /// Activates the lock, clearing previous data and starting countdown.
//        /// </summary>
//        public void Activate()
//        {
//            _collectedPoints.Clear(); 
//            _isLocked = true;
//            _timer.Stop();
//            _timer.Start();
//        }

//        /// <summary>
//        /// Processes and stores a gaze point if the lock is active and the data is valid.
//        /// </summary>
//        /// <param name="x">The normalized X coordinate.</param>
//        /// <param name="y">The normalized Y coordinate.</param>
//        /// <param name="bpogv">The validity flag from the eye-tracker (1 = valid).</param>
//        public void ProcessPoint(double x, double y, int bpogv)
//        {
//            if (!_isLocked) return;

//            if (bpogv == 1)
//            {
//                _collectedPoints.Add(new Point(x, y));
//            }
//        }

//        private void ReleaseLock()
//        {
//            _timer.Stop();
//            _isLocked = false;

//            OnLockExpired?.Invoke(new List<Point>(_collectedPoints));

//            _collectedPoints.Clear();
//        }
//    }
//}

