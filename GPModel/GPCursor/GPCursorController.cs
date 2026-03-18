using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Gaze_Point.Services
{
    public class GPCursorController
    {
        private readonly GazeCursorWindow _window;
        private const double HalfSize = 20;

        // --- WIN32 API CORRETTA ---
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        // --------------------------

        public GPCursorController()
        {
            _window = new GazeCursorWindow();
        }

        public void Show() => _window.Show();
        public void Hide() => _window.Hide();

        public void UpdatePosition(double logX, double logY)
        {
            // 1. Spostamento standard WPF
            _window.Left = logX - HalfSize;
            _window.Top = logY - HalfSize;

            // 2. FORZA IL TOPMOST SOPRA LE POPUP TRAMITE WIN32
            var hwnd = new WindowInteropHelper(_window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                // SetWindowPos con HWND_TOPMOST garantisce che la finestra sia 
                // in cima alla catena di rendering di Windows, sopra le Popup di WPF.
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
        }
    }
}


