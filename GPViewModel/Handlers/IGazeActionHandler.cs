using System.Windows;
using Gaze_Point.Services;

namespace Gaze_Point.GPViewModel.Handlers
{
    public interface IGazeActionHandler
    {
        FrameworkElement Execute(FrameworkElement element, GPService service);
    }
}

