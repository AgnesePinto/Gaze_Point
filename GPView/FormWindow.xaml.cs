using Gaze_Point.GPViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gaze_Point.GPView
{
    /// <summary>
    /// Interaction Logic FormWindow.xaml
    /// </summary>
    public partial class FormWindow : Window
    {
        public FormWindow()
        {
            InitializeComponent();

            EventManager.RegisterClassHandler(typeof(FrameworkElement),
            FrameworkElement.GotFocusEvent, new RoutedEventHandler(OnElementFocusedByKeyboard));
        }

        private void OnElementFocusedByKeyboard(object sender, RoutedEventArgs e)
        {
            if (DataContext is FormViewModel vm && e.OriginalSource is FrameworkElement fe)
            {
                vm.SetFocusedElement(fe.Name);
            }
        }
    }
}
