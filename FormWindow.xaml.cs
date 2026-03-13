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

namespace Gaze_Point
{
    /// <summary>
    /// Logica di interazione per FormWindow.xaml
    /// </summary>
    public partial class FormWindow : Window
    {
        public FormWindow()
        {
            InitializeComponent();

            EventManager.RegisterClassHandler(typeof(FrameworkElement),
            FrameworkElement.GotFocusEvent, new RoutedEventHandler(OnElementFocusedByKeyboard));
        }

        // 2. IL METODO "PONTE" (fuori dal costruttore)
        private void OnElementFocusedByKeyboard(object sender, RoutedEventArgs e)
        {
            // Il Code-behind passa l'informazione al ViewModel
            if (DataContext is FormViewModel vm && e.OriginalSource is FrameworkElement fe)
            {
                vm.SetFocusedElement(fe.Name);
            }
        }
    }
}
