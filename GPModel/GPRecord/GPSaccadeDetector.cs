using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IO;

namespace Gaze_Point.GPModel.GPRecord
{

    /// <summary>
    /// Evaluates gaze data to detect significant saccadic movements.
    /// Distinguishes btween intentional rapid eye shifts and involuntary micro-oscillations based on magnitude thresholds defined in pixels.
    /// </summary>
    /// <author>Agnese Pinto</author>
     

    public class GPSaccadeDetector
    {
        private readonly double _saccadeThresholdMin;
        private readonly double _saccadeThresholdMax;

        public GPSaccadeDetector()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                _saccadeThresholdMin = double.Parse(config["Saccade:SaccadeThresholdMin"], CultureInfo.InvariantCulture);
                _saccadeThresholdMax = double.Parse(config["Saccade:SaccadeThresholdMax"], CultureInfo.InvariantCulture);
            }
            catch
            {
                // Fallback
                _saccadeThresholdMin = 30.0; 
                _saccadeThresholdMax = 100.0; 
            }
        }


        /// <summary>
        /// Analyzes the saccade magnitude of the provided data record to determine if an intentional gaze shift has occurred.
        /// </summary>
        /// <param name="data">The gaze data record containign saccade metrics.</param>
        /// <returns>True if the movement magnitude falls within the defines intentional range; otherwise false.</returns>
        public bool IsSignificantSaccade(GPData data)
        {
            if (data != null)
            {
                if(data.SACCADE_MAG > _saccadeThresholdMin && data.SACCADE_MAG < _saccadeThresholdMax)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
