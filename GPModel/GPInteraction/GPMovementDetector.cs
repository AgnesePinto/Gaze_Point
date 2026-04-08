using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace Gaze_Point.GPModel.GPInteraction
{

    /// <summary>
    /// Analyzes a collection of gaze points to determine the type of eye movement performed.
    /// Categorizes movements into static stayes, intentional small steps or large saccadic jumps.
    /// </summary>
    /// <author>Agnese Pinto</author>
     

    public class GPMovementDetector
    {
        public readonly double _largeMovementThreshold;
        public readonly double _smallMovementThreshold;

        /// <summary>
        /// Defines the possible categories of detected gaze movements.
        /// </summary>
        public enum MovementType { Stay, SmallStep, LargeJump }

        /// <summary>
        /// Contains the results of a movement analysis, including type, angle and distance.
        /// </summary>
        public struct MovementAnalysis
        {
            public MovementType Type;
            public double Angle;
            public double Distance;
        }

        public GPMovementDetector() 
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                _largeMovementThreshold = double.Parse(config["MovementDetector:LargeMovementThreshold"], CultureInfo.InvariantCulture);
                _smallMovementThreshold = double.Parse(config["MovementDetector:SmallMovementThreshold"], CultureInfo.InvariantCulture);
            }
            catch
            {
                // Fallback
                _largeMovementThreshold = 0.10;
                _smallMovementThreshold = 0.03;
    }
        }


        /// <summary>
        /// Evaluates a list of gaze points to identify the movement pattern.
        /// </summary>
        /// <param name="points">The list of normalized points collected during a look period.</param>
        /// <returns>A movement analysis structure describing the detected movement.</returns>
        public MovementAnalysis Analyze(List<Point> points)
        {
            if (points == null || points.Count < 10) 
                return new MovementAnalysis { Type = MovementType.Stay };

            Point start = points.First();

            double sumX = 0;
            double sumY = 0;
            for (int i = 1; i < points.Count; i++)
            {
                sumX += points[i].X;
                sumY += points[i].Y;
            }

            Point averagePoint = new Point(sumX / (points.Count - 1), sumY / (points.Count - 1));

            double dx = averagePoint.X - start.X;
            double dy = averagePoint.Y - start.Y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (angle < 0) angle += 360;

            if (angle < 0) angle += 360;

            if (distance > _largeMovementThreshold)
                return new MovementAnalysis { Type = MovementType.LargeJump, Distance = distance };

            if (distance >= _smallMovementThreshold && distance <= _largeMovementThreshold)
                return new MovementAnalysis { Type = MovementType.SmallStep, Angle = angle, Distance = distance };

            return new MovementAnalysis { Type = MovementType.Stay, Distance = distance };
        }

    }
}

