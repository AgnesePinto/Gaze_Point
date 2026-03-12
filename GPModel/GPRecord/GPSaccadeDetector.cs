using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaze_Point.GPModel.GPRecord
{
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

                _saccadeThresholdMin = double.Parse(config["Interaction:SaccadeThresholdMin"]);
                _saccadeThresholdMax = double.Parse(config["Interaction:SaccadeThresholdMax"]);
            }
            catch
            {
                _saccadeThresholdMin = 70.0; // Fallback
                _saccadeThresholdMax = 200.0; // Fallback
            }
        }

        // Rileva uno spostamento intenzionale rilevante
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
