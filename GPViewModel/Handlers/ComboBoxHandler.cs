using Gaze_Point.GPViewModel.Handlers;
using Gaze_Point.Services;
using System.Windows;
using System.Windows.Controls;

namespace Gaze_Point.GPViewModel.Handlers
{
    public class ComboBoxHandler : IGazeActionHandler
    {
        public FrameworkElement Execute(FrameworkElement element, GPService service)
        {
            if (element is ComboBox cb)
            {
                cb.IsDropDownOpen = !cb.IsDropDownOpen;
                cb.Focus();
                if (cb.IsDropDownOpen) service.RefreshInteractionTargets();
                return cb;
            }

            if (element is ComboBoxItem item)
            {
                var parentCombo = ItemsControl.ItemsControlFromItemContainer(item) as ComboBox;
                if (parentCombo != null)
                {
                    parentCombo.SelectedItem = item;
                    parentCombo.IsDropDownOpen = false;
                    parentCombo.Focus();
                    service.ResetInteractionState();
                    return parentCombo;
                }
            }
            return element;
        }
    }
}

