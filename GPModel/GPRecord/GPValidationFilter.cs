using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Gaze_Point.GPModel.GPRecord 
{ 
    /// <summary> 
    /// Handles gaze data validation, blink recovery management and spatial constraints. 
    /// Ensures signal continuity by masking loss of tracking periods and clamping coordinates within a safe visual range. 
    /// </summary> 
    /// <remarks> 
    /// This filter implements a "Blanking Period" to ignore unstable samples immediatly after the eyes re-open. 
    /// </remarks> 
    /// <author>Agnese Pinto</author> 
    public class GPValidationFilter 
    { 
        private readonly int _recoverySamples; 
        private readonly double _minRange; 
        private readonly double _maxRange; 
        private double _lastValidX = 0.5; 
        private double _lastValidY = 0.5; 
        private int _recoveryCounter = 0; 
        private bool _wasValid = true; 
        public GPValidationFilter() 
        { 
            try 
            { 
                var config = new ConfigurationBuilder() 
                    .SetBasePath(Directory.GetCurrentDirectory()) 
                    .AddJsonFile("AppSettings/DataSettings.json") 
                    .Build(); 
                _minRange = double.Parse(config["Validation:MinRange"]); 
                _maxRange = double.Parse(config["Validation:MaxRange"]); 
                _recoverySamples = int.Parse(config["Validation:SamplesBlankingPeriod"]); 
            } 
            catch 
            { 
                // Fallback
                _minRange = 0.01; 
                _maxRange = 0.99; 
                _recoverySamples = 3; 
            } 
        } 
        
        public GPData ValidationFilter(GPData rawData) 
        { 
            if (rawData == null) return null; 
            BlinkRecovery(rawData); 
            if(!IsValid(rawData) || _recoveryCounter > 0) 
            { 
                FreezeToLastValid(rawData); 
            } 
            else 
            { 
                UpdateData(rawData); 
            } 
            Clamping(rawData); 
            return rawData; 
        } 
        
        /// <summary> 
        /// Detects the transition from an invalid state (eye closed) to a valid state and initializes the recovery sample counter. 
        /// </summary> 
        private void BlinkRecovery(GPData data) 
        { 
            bool currentValid = IsValid(data); 
            if(currentValid && !_wasValid) 
            { 
                _recoveryCounter = _recoverySamples; 
            } 
            _wasValid = currentValid; 
        } 
        
        /// <summary> 
        /// Replaces current coordinates with the last known stable position and manage the countdown for the recovery period. 
        /// </summary> 
        private void FreezeToLastValid(GPData data) 
        { 
            data.BPOGX = _lastValidX; 
            data.BPOGY = _lastValidY; 
            data.BPOGV = 1; 
            
            if(_recoveryCounter > 0) 
            { 
                _recoveryCounter--; 
            } 
        } 
        
        /// <summary> 
        /// Updates the internal memory with the most recent stable gaze coordinates. 
        /// </summary> 
        private void UpdateData(GPData data) 
        { 
            _lastValidX = data.BPOGX; 
            _lastValidY = data.BPOGY; 
        } 
        
        /// <summary> 
        /// Restricts gaze coordinates within the configurated safe boundaries to prevent the cursor from leaving the designed screen area. 
        /// </summary> 
        private void Clamping(GPData data) 
        { 
            data.BPOGX = Math.Max(_minRange, Math.Min(_maxRange, data.BPOGX)); 
            data.BPOGY = Math.Max(_minRange, Math.Min(_maxRange, data.BPOGY)); 
        } 
        
        private bool IsValid(GPData rawData) 
        { 
            return rawData.BPOGV == 1; 
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
//    /// Handles gaze data validation, blink recovery management and spatial constraints.
//    /// Ensures signal continuity by masking loss of tracking periods and clamping coordinates within a safe visual range.
//    /// </summary>
//    /// <remarks>
//    /// This filter implements a "Blanking Period" to ignore unstable samples immediatly after the eyes re-open.
//    /// </remarks>
//    /// <author>Agnese Pinto</author>


//    public class GPValidationFilter
//    {
//        private readonly int _recoverySamples;
//        private readonly double _minRange;
//        private readonly double _maxRange;

//        private double _lastValidX = 0.5;
//        private double _lastValidY = 0.5;
//        private int _recoveryCounter = 0;
//        private bool _wasValid = true;


//        public GPValidationFilter()
//        {
//            try
//            {
//                var config = new ConfigurationBuilder()
//                    .SetBasePath(Directory.GetCurrentDirectory())
//                    .AddJsonFile("AppSettings/DataSettings.json")
//                    .Build();

//                _minRange = double.Parse(config["Validation:MinRange"], CultureInfo.InvariantCulture);
//                _maxRange = double.Parse(config["Validation:MaxRange"], CultureInfo.InvariantCulture);
//                _recoverySamples = int.Parse(config["Validation:SamplesBlankingPeriod"], CultureInfo.InvariantCulture);
//            }
//            catch
//            {
//                // Fallback
//                _minRange = 0.01;
//                _maxRange = 0.99;
//                _recoverySamples = 3;
//            }
//        }

//        public GPData ValidationFilter(GPData rawData)
//        {
//            if (rawData == null) return null;

//            BlinkRecovery(rawData);

//            if(!IsValid(rawData) || _recoveryCounter > 0)
//            {
//                FreezeToLastValid(rawData);
//            }
//            else
//            {
//                UpdateData(rawData);
//            }

//            Clamping(rawData);

//            return rawData;
//        }


//        /// <summary>
//        /// Detects the transition from an invalid state (eye closed) to a valid state and initializes the recovery sample counter.
//        /// </summary>
//        private void BlinkRecovery(GPData data)
//        {
//            bool currentValid = IsValid(data);

//            if(currentValid && !_wasValid)
//            {
//                _recoveryCounter = _recoverySamples;
//            }
//            _wasValid = currentValid;
//        }


//        /// <summary>
//        /// Replaces current coordinates with the last known stable position and manage the countdown for the recovery period.
//        /// </summary>
//        private void FreezeToLastValid(GPData data)
//        {
//            data.BPOGX = _lastValidX;
//            data.BPOGY = _lastValidY;
//            data.BPOGV = 1;

//            if(_recoveryCounter > 0)
//            {
//                _recoveryCounter--;
//            }
//        }


//        /// <summary>
//        /// Updates the internal memory with the most recent stable gaze coordinates.
//        /// </summary>
//        private void UpdateData(GPData data)
//        {
//            _lastValidX = data.BPOGX;
//            _lastValidY = data.BPOGY;
//        }


//        /// <summary>
//        /// Restricts gaze coordinates within the configurated safe boundaries to prevent the cursor from leaving the designed screen area.
//        /// </summary>
//        private void Clamping(GPData data)
//        {
//            data.BPOGX = Math.Max(_minRange, Math.Min(_maxRange, data.BPOGX));
//            data.BPOGY = Math.Max(_minRange, Math.Min(_maxRange, data.BPOGY));
//        }


//        private bool IsValid(GPData rawData)
//        {
//            return rawData.BPOGV == 1;
//        }
//    }
//}


