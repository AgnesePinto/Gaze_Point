using Gaze_Point.Services;
using System.Windows;
using System.Windows.Controls;

namespace Gaze_Point.GPViewModel.Handlers
{
    public class StandardControlHandler : IGazeActionHandler
    {
        public FrameworkElement Execute(FrameworkElement element, GPService service)
        {
            element.Focus();

            switch (element)
            {
                case Button b:
                    b.Command?.Execute(b.CommandParameter);
                    break;
                case RadioButton rb:
                    rb.IsChecked = true;
                    break;
                case CheckBox chk:
                    chk.IsChecked = !chk.IsChecked;
                    break;
            }
            return element;
        }
    }
}


