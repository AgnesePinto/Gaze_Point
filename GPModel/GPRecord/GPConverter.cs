using System.Windows;

namespace Gaze_Point.GPModel.GPRecord 
{ 

    /// <summary>
    /// Provides spatial transformation methods to convert normalized gaze data into physical, 
    /// logical and window-relative coordinate systems. 
    /// </summary> 
    /// <author>Agnese Pinto</author> 
    

    public static class GPConverter 
    {

        /// <summary> 
        /// Converts normalized gaze coordinates (0.0 to 1.0) into WPF logical pixels (DPI-indipendent). 
        /// </summary> 
        /// <param name="data">The normalized gaze data record.</param> 
        /// <returns>A tuple containing the X and Y coordinates in logical pixels.</returns> 
        public static (double X, double Y) ToLogicalScreenPoint(GPData data)
        {
            if (data == null) return (0.0, 0.0);
            double logX = data.BPOGX * SystemParameters.PrimaryScreenWidth;
            double logY = data.BPOGY * SystemParameters.PrimaryScreenHeight;
            return (logX, logY);
        }


        /// <summary> 
        /// Converts normalized gaze coordinates (0.0 to 1.0) into physical screen pixels. 
        /// </summary> 
        /// <param name="data">The normalized gaze data record.</param> 
        /// <returns>A tuple containing the X and Y coordinates in physical pixels.</returns> 
        public static (int X, int Y) ToPhysicalScreenPoint(GPData data) 
        { 
            if (data == null) return (0, 0); 
            double scaleX = 1.0; 
            double scaleY = 1.0; 
            if (Application.Current.MainWindow != null) 
            { 
                var source = PresentationSource.FromVisual(Application.Current.MainWindow); 
                if (source?.CompositionTarget != null) 
                { 
                    scaleX = source.CompositionTarget.TransformToDevice.M11; 
                    scaleY = source.CompositionTarget.TransformToDevice.M22; 
                } 
            } 
            int physX = (int)(data.BPOGX * SystemParameters.PrimaryScreenWidth * scaleX); 
            int physY = (int)(data.BPOGY * SystemParameters.PrimaryScreenHeight * scaleY); 
            return (physX, physY); 
        } 
        

        /// <summary> 
        /// Translates a physical screen point into a point relative to the coordinates system of a specific window. 
        /// </summary> 
        /// <param name="physicalPoint">The screen point in physical pixels.</param> 
        /// <param name="window">The target window for coordinate mapping.</param> 
        /// <returns>A point object relative to window's top-left corner.</returns> 
        public static Point ToWindowPoint(Point physicalPoint, Window window) 
        { 
            if (window == null) return (new Point(0, 0)); 
            return window.PointFromScreen(physicalPoint); 
        } 
        
    }
}
