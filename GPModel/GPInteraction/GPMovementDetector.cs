using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPMovementDetector
    {
        // Soglie da mettere poi nel JSON (0.1 = 10% dello schermo)
        private const double LargeMovementThreshold = 0.20;
        private const double SmallMovementThreshold = 0.03;

        public enum MovementType { Stay, SmallStep, LargeJump }

        public struct AnalysisResult
        {
            public MovementType Type;
            public double Angle; // In gradi (0-360)
            public double Distance;
        }

        public AnalysisResult Analyze(List<Point> points)
        {
            if (points == null || points.Count < 2)
                return new AnalysisResult { Type = MovementType.Stay };

            Point start = points.First();
            Point end = points.Last(); // O la media degli ultimi 10 punti per stabilità

            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            // Calcolo angolo in gradi (0 è destra, 90 è giù, 180 sinistra, 270 su)
            double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (angle < 0) angle += 360;

            if (distance > LargeMovementThreshold)
                return new AnalysisResult { Type = MovementType.LargeJump, Distance = distance };

            if (distance >= SmallMovementThreshold && distance <= LargeMovementThreshold)
                return new AnalysisResult { Type = MovementType.SmallStep, Angle = angle, Distance = distance };

            return new AnalysisResult { Type = MovementType.Stay, Distance = distance };
        }
    }
}

