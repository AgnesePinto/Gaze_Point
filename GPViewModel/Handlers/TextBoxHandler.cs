using Gaze_Point.Services;
using System.Windows;
using System.Windows.Controls;

namespace Gaze_Point.GPViewModel.Handlers
{
    public class TextBoxHandler : IGazeActionHandler
    {
        public FrameworkElement Execute(FrameworkElement element, GPService service)
        {
            if (element is TextBox tb)
            {
                tb.Focus();
                tb.CaretIndex = tb.Text.Length;
            }
            return element;
        }
    }
}


