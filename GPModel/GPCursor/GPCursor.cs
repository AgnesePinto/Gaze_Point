using System.ComponentModel;

namespace Gaze_Point.GPModel.GPCursor
{
    public class GPCursor : INotifyPropertyChanged
    {
        private double _x;
        private double _y;

        // Posizione X calcolata per l'interfaccia (Pixel Logici)
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(nameof(X)); }
        }

        // Posizione Y calcolata per l'interfaccia (Pixel Logici)
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(nameof(Y)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
