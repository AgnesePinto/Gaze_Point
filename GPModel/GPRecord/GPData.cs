using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaze_Point.GPModel.GPRecord
{
    public class GPData
    {
        // Coordinate dello sguardo (da 0.0 a 1.0)
        public double BPOGX { get; set; }
        public double BPOGY { get; set; }

        // Validità del dato (1 = ok, 0 = perso)
        public int BPOGV { get; set; }

        // Tempo di sistema del record
        public double TIME { get; set; }

        // Coordinate del mouse reale 
        public double CX { get; set; }
        public double CY { get; set; }

        // Coordinate dei punti di fissazione
        public double FPOGX { get; set; }
        public double FPOGY { get; set; }

        // Saccadi
        public double SACCADE_MAG { get; set; }
        public double SACCADE_DIR { get; set; }
    }
}
