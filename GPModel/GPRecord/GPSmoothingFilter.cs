using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;

namespace Gaze_Point.GPModel.GPRecord 
{ 
    /// <summary> 
    /// Applies an adaptive firs-order low-pass filter to gaze data to reduce jitter. 
    /// Dinamically adjusts smoothing (Alpha) based on the distance between successive points to balance stability and responsivness. 
    /// </summary> 
    /// <author>Agnese Pinto</author> 
    public class GPSmoothingFilter 
    { 
        private readonly double _alphaMin; 
        private readonly double _alphaMax; 
        private readonly double _sensitivity; 
        private readonly double _distanceThreshold; 
        private double _lastX = 0.5; 
        private double _lastY = 0.5; 
        public GPSmoothingFilter() 
        { 
            try 
            { 
                var config = new ConfigurationBuilder() 
                    .SetBasePath(Directory.GetCurrentDirectory()) 
                    .AddJsonFile("AppSettings/DataSettings.json") 
                    .Build(); 
                _alphaMin = double.Parse(config["Smoothing:AlphaMin"], CultureInfo.InvariantCulture); 
                _alphaMax = double.Parse(config["Smoothing:AlphaMax"], CultureInfo.InvariantCulture); 
                _sensitivity = double.Parse(config["Smoothing:Sensitivity"], CultureInfo.InvariantCulture); 
                _distanceThreshold = double.Parse(config["Smoothing:DistanceThreshold"], CultureInfo.InvariantCulture); 
            } 
            catch 
            { 
                // Fallback
                _alphaMin = 0.009; 
                _alphaMax = 0.09; 
                _sensitivity = 0.6; 
                _distanceThreshold = 0.02; 
            } 
        } 
        
        /// <summary> 
        /// Orchestrates the adaptive smoothing process on the provided gaze data. 
        /// </summary> 
        /// <param name="data">The gaze data record to be smoothed.</param> 
        /// <returns>The processed GPData with filtered coordinates.</returns> 
        public GPData AdaptiveSmoothing(GPData data) 
        { 
            if (data == null) return null; 
            double distance = ComputeDistance(data.BPOGX, data.BPOGY); 
            if(distance > _distanceThreshold) 
            { 
                double alpha = CalculateDynamicAlpha(distance); 
                ApplyFilter(data, alpha); 
            } 
            else 
            { 
                data.BPOGX = _lastX; 
                data.BPOGY = _lastY; 
            } 
            return data; 
        } 
        
        /// <summary> 
        /// Determines the filter coefficient (Alpha) based on movement magnitude, clamped between configured minimum and maximum limits. 
        /// </summary> 
        private double CalculateDynamicAlpha(double distance) 
        { 
            double alpha = _alphaMin + (distance * _sensitivity); 
            return Math.Max(_alphaMin, Math.Min(_alphaMax, alpha)); 
        } 
        
        /// <summary> 
        /// Calculates the weighted avarage between the current gaze point and the previous position to reduce sudden jumps in the signal. 
        /// </summary> 
        /// <param name="data">The gaze data object to be modified with smoothed coordinates.</param> 
        /// <param name="alpha">The dynamic weighting factor (0.0 to 1.0) defining filter strenght.</param> 
        private void ApplyFilter(GPData data, double alpha) 
        { 
            double smoothX = (alpha * data.BPOGX) + ((1 - alpha) * _lastX); 
            double smoothY = (alpha * data.BPOGY) + ((1 - alpha) * _lastY); 
            _lastX = smoothX; 
            _lastY = smoothY;
            data.BPOGX = smoothX; 
            data.BPOGY = smoothY; 
        } 
        
        private double ComputeDistance(double newX, double newY) 
        { 
            double dx = newX - _lastX; 
            double dy = newY - _lastY; 
            return Math.Sqrt(dx * dx + dy * dy); 
        } 
    }
}



//using System;
//using System.Globalization;
//using System.IO;
//using Microsoft.Extensions.Configuration;

//namespace Gaze_Point.GPModel.GPRecord
//{

//    /// <summary>
//    /// Applies an adaptive firs-order low-pass filter to gaze data to reduce jitter.
//    /// Dinamically adjusts smoothing (Alpha) based on the distance between successive points to balance stability and responsivness.
//    /// </summary>
//    /// <author>Agnese Pinto</author>


//    public class GPSmoothingFilter
//    {
//        private readonly double _alphaMin;
//        private readonly double _alphaMax;
//        private readonly double _sensitivity;
//        private readonly double _distanceThreshold;

//        private double _lastX = 0.5;
//        private double _lastY = 0.5;


//        public GPSmoothingFilter()
//        {
//            try
//            {
//                var config = new ConfigurationBuilder()
//                    .SetBasePath(Directory.GetCurrentDirectory())
//                    .AddJsonFile("AppSettings/DataSettings.json")
//                    .Build();

//                _alphaMin = double.Parse(config["Smoothing:AlphaMin"], CultureInfo.InvariantCulture);
//                _alphaMax = double.Parse(config["Smoothing:AlphaMax"], CultureInfo.InvariantCulture);
//                _sensitivity = double.Parse(config["Smoothing:Sensitivity"], CultureInfo.InvariantCulture);
//                _distanceThreshold = double.Parse(config["Smoothing:DistanceThreshold"], CultureInfo.InvariantCulture);
//            }
//            catch
//            {
//                // Fallback
//                _alphaMin = 0.009;
//                _alphaMax = 0.09;
//                _sensitivity = 0.6;
//                _distanceThreshold = 0.02;
//            }
//        }


//        /// <summary>
//        /// Orchestrates the adaptive smoothing process on the provided gaze data.
//        /// </summary>
//        /// <param name="data">The gaze data record to be smoothed.</param>
//        /// <returns>The processed GPData with filtered coordinates.</returns>
//        public GPData AdaptiveSmoothing(GPData data)
//        {
//            if (data == null) return null;

//            double distance = ComputeDistance(data.BPOGX, data.BPOGY);

//            if(distance > _distanceThreshold)
//            {
//                double alpha = CalculateDynamicAlpha(distance);
//                ApplyFilter(data, alpha);
//            }
//            else
//            {
//                data.BPOGX = _lastX;
//                data.BPOGY = _lastY;
//            }
//            return data;
//        }


//        /// <summary>
//        /// Determines the filter coefficient (Alpha) based on movement magnitude, clamped between configured minimum and maximum limits. 
//        /// </summary>
//        private double CalculateDynamicAlpha(double distance)
//        {
//            double alpha = _alphaMin + (distance * _sensitivity);

//            return Math.Max(_alphaMin, Math.Min(_alphaMax, alpha));
//        }


//        /// <summary>
//        /// Calculates the weighted avarage between the current gaze point and the previous position to reduce sudden jumps in the signal.
//        /// </summary>
//        /// <param name="data">The gaze data object to be modified with smoothed coordinates.</param>
//        /// <param name="alpha">The dynamic weighting factor (0.0 to 1.0) defining filter strenght.</param>
//        private void ApplyFilter(GPData data, double alpha)
//        {
//            double smoothX = (alpha * data.BPOGX) + ((1 - alpha) * _lastX);
//            double smoothY = (alpha * data.BPOGY) + ((1 - alpha) * _lastY);

//            _lastX = smoothX;
//            _lastY = smoothY;

//            data.BPOGX = smoothX;
//            data.BPOGY = smoothY;
//        }


//        private double ComputeDistance(double newX, double newY)
//        {
//            double dx = newX - _lastX;
//            double dy = newY - _lastY;

//            return Math.Sqrt(dx * dx + dy * dy);
//        }

//    }
//}
