using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPMovementDetector
    {
        // Soglie normalizzate
        public readonly double _largeMovementThreshold;
        public readonly double _smallMovementThreshold;

        public enum MovementType { Stay, SmallStep, LargeJump }

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

                _largeMovementThreshold = double.Parse(config["Interaction:LargeMovementThreshold"]);
                _smallMovementThreshold = double.Parse(config["Interaction:SmallMovementThreshold"]);
            }
            catch
            {
                _largeMovementThreshold = 0.20;
                _smallMovementThreshold = 0.03;
    }
        }

        public MovementAnalysis Analyze(List<Point> points)
        {
            if (points == null || points.Count < 10) // Serve un minimo di campioni
                return new MovementAnalysis { Type = MovementType.Stay };

            Point start = points.First();

            // 1. PUNTO MEDIO (Baricentro) di tutti i punti successivi
            double sumX = 0;
            double sumY = 0;
            for (int i = 1; i < points.Count; i++)
            {
                sumX += points[i].X;
                sumY += points[i].Y;
            }

            Point averagePoint = new Point(sumX / (points.Count - 1), sumY / (points.Count - 1));

            // 2. Vettore tra START e il PUNTO MEDIO
            double dx = averagePoint.X - start.X;
            double dy = averagePoint.Y - start.Y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            // 3. Calcolo angolo 
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

