using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaze_Point.GPModel.GPRecord
{

    /// <summary>
    /// Represents a single data record from the Gazepoint eye-tracker.
    /// Contains normalized gaze coordinates, data validity and saccade metrics.
    /// </summary>
    /// <remarks>
    /// Coordinates are normalized (0.0 to 1.0), while saccade magnitude is measured in pixels.
    /// </remarks>
    /// <author>Agnese Pinto</author>
    public class GPData
    {

        /// <summary>
        /// Best Point of Gaze X-coordinate (normalized 0.0 to 1.0).
        /// </summary>
        public double BPOGX { get; set; }

        /// <summary>
        /// Best Point of Gaze Y-coordinate (normalized 0.0 to 1.0).
        /// </summary>
        public double BPOGY { get; set; }

        /// <summary>
        /// Gaze data validity flag (1 = Valid, 0 = Invalid/Lost).
        /// </summary>
        public int BPOGV { get; set; }

        /// <summary>
        /// Saccade magnitude, representing the displacement in pixels between successive fixation points.
        /// </summary>
        public double SACCADE_MAG { get; set; }
    }
}
