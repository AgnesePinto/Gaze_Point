using System.Windows;

namespace Gaze_Point.GPViewModel.Handlers
{
    public interface IGazeFocusHandler
    {
        void OnFocus(FrameworkElement element);
    }
}
