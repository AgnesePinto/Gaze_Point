using Gaze_Point.GPView;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
namespace Gaze_Point.Services 
{ 
    /// <summary> 
    /// Manages the visual gaze cursor window, handling its lifecycle, position update, 
    /// and ensuring it remains the topmost element on the screen using Win32 API. 
    /// </summary> 
    /// <author>Agnese Pinto</author> 
    public class GPCursorController 
    { 
        private readonly GazeCursorWindow _window; 
        private readonly double _halfSize; 
        
        // --- WIN32 API INTEROP ---
        [DllImport("user32.dll", SetLastError = true)] 
        [return: MarshalAs(UnmanagedType.Bool)] 
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags); 
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); 
        private const uint SWP_NOMOVE = 0x0002; 
        private const uint SWP_NOSIZE = 0x0001; 
        private const uint SWP_NOACTIVATE = 0x0010; 
        private const uint SWP_SHOWWINDOW = 0x0040; 
        // --------------------------
        public GPCursorController() { _window = new GazeCursorWindow(); 
            try 
            { 
                var config = new ConfigurationBuilder() 
                    .SetBasePath(Directory.GetCurrentDirectory()) 
                    .AddJsonFile("AppSettings/DataSettings.json") 
                    .Build(); 
                double _cursorSize = double.Parse(config["Cursor:CursorSize"], CultureInfo.InvariantCulture); 
                _halfSize = _cursorSize / 2.0; 
            } 
            catch 
            { 
                // Fallback
                _halfSize = 20.0; 
            } 
        } 
        
        /// <summary> 
        /// Updates cursor's screen position and forces it to stay above all other visual elements. 
        /// </summary> 
        /// <param name="logX">The horizontal gaze coordinate in logical pixels.</param> 
        /// <param name="logY">The vertical gaze coordinate in logical pixels.</param> 
        /// <remarks> Uses Win32 SetWindowPos with HWND_TOPMOST to bypass WPF Popup rendering limitations.
        /// </remarks> 
        public void UpdatePosition(double logX, double logY) 
        { 
            _window.Left = logX - _halfSize; 
            _window.Top = logY - _halfSize; 
            var hwnd = new WindowInteropHelper(_window).Handle; 
            if (hwnd != IntPtr.Zero) 
            { 
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW); 
            } 
        } 
        public void Show() => _window.Show(); public void Hide() => _window.Hide(); 
    }
}


//using Microsoft.Extensions.Configuration;
//using System;
//using System.Runtime.InteropServices;
//using System.Windows;
//using System.Windows.Interop;
//using System.IO;
//using Gaze_Point.GPView;
//using System.Globalization;

//namespace Gaze_Point.Services
//{

//    /// <summary>
//    /// Manages the visual gaze cursor window, handling its lifecycle, position update,
//    /// and ensuring it remains the topmost element on the screen using Win32 API.
//    /// </summary>
//    /// <author>Agnese Pinto</author>


//    public class GPCursorController
//    {
//        private readonly GazeCursorWindow _window;
//        private readonly double _halfSize;


//        // --- WIN32 API INTEROP ---
//        [DllImport("user32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

//        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
//        private const uint SWP_NOMOVE = 0x0002;
//        private const uint SWP_NOSIZE = 0x0001;
//        private const uint SWP_NOACTIVATE = 0x0010;
//        private const uint SWP_SHOWWINDOW = 0x0040;
//        // --------------------------


//        public GPCursorController()
//        {
//            _window = new GazeCursorWindow();

//            try
//            {
//                var config = new ConfigurationBuilder()
//                    .SetBasePath(Directory.GetCurrentDirectory())
//                    .AddJsonFile("AppSettings/DataSettings.json")
//                    .Build();

//                double _cursorSize = double.Parse(config["Cursor:CursorSize"], CultureInfo.InvariantCulture);
//                _halfSize = _cursorSize / 2.0;
//            }
//            catch 
//            {
//                // Fallback
//                _halfSize = 20.0;
//            }
//        }


//        /// <summary>
//        /// Updates cursor's screen position and forces it to stay above all other visual elements.
//        /// </summary>
//        /// <param name="logX">The horizontal gaze coordinate in logical pixels.</param>
//        /// <param name="logY">The vertical gaze coordinate in logical pixels.</param>
//        /// <remarks>
//        /// Uses Win32 SetWindowPos with HWND_TOPMOST to bypass WPF Popup rendering limitations.
//        /// </remarks>
//        public void UpdatePosition(double logX, double logY)
//        {
//            _window.Left = logX - _halfSize;
//            _window.Top = logY - _halfSize;

//            var hwnd = new WindowInteropHelper(_window).Handle;
//            if (hwnd != IntPtr.Zero)
//            {
//                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
//                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
//            }
//        }

//        public void Show() => _window.Show();
//        public void Hide() => _window.Hide();
//    }
//}


