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
    public class ComboBoxHandler : IGazeActionHandler, IGazeFocusHandler
    {
        public void OnFocus(FrameworkElement element)
        {
            if (element is ComboBoxItem item)
            {
                item.IsSelected = false;
                item.Focus();
            }
        }

        public void Execute(FrameworkElement element, GPService service)
        {
            if (element is ComboBox cb)
            {
                cb.IsDropDownOpen = !cb.IsDropDownOpen;
                cb.Focus();
                if (cb.IsDropDownOpen) service.RefreshInteractionTargets();
            }
            else if (element is ComboBoxItem item)
            {
                var parent = ItemsControl.ItemsControlFromItemContainer(item) as ComboBox;
                if (parent != null)
                {
                    parent.SelectedItem = item;
                    parent.IsDropDownOpen = false;
                    parent.Focus();
                    service.ResetInteractionState();
                }
            }
        }
    }
}
