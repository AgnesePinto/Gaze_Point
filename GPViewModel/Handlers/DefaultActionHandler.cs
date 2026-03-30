using Gaze_Point.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gaze_Point.GPViewModel.Handlers
{
    public class DefaultActionHandler : IGazeActionHandler
    {
        public void Execute(FrameworkElement element, GPService service)
        {
            element.Focus();
            if (element is Button b) b.Command?.Execute(b.CommandParameter);
            else if (element is RadioButton rb) rb.IsChecked = true;
            else if (element is CheckBox cb) cb.IsChecked = !cb.IsChecked;
        }
    }
}
