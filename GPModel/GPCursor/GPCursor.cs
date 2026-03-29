using System.ComponentModel;

namespace Gaze_Point.GPModel.GPCursor
{

    /// <summary>
    /// Represents the visual gaze cursor state for UI binding.
    /// Implements INotifyPropertyChanged to enable real-time position updates on the screen.
    /// </summary>
    /// <author>Agnese Pinto</author>
    public class GPCursor : INotifyPropertyChanged
    {
        private double _x;
        private double _y;


        /// <summary>
        /// Gets or set the X-coordinate position in logical pixels (DPI-independent).
        /// </summary>
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(nameof(X)); }
        }


        /// <summary>
        /// Gets or set the Y-coordinate position in logical pixels (DPI-independent).
        /// </summary>
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(nameof(Y)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
