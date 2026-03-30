using Gaze_Point.Services;
using System.Windows;

namespace Gaze_Point.GPViewModel.Handlers
{
    public interface IGazeActionHandler
    {
        void Execute(FrameworkElement element, GPService service);
    }
}
