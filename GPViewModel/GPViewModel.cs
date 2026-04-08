using Gaze_Point.GPModel.GPCursor;
using Gaze_Point.GPModel.GPRecord;
using Gaze_Point.GPView;
using Gaze_Point.GPViewModel.Handlers;
using Gaze_Point.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gaze_Point.GPViewModel
{
    /// <summary>
    /// Coordinates the main application logic and handles navigation between the main screen and the form.
    /// Manages high-level commands for starting and stopping the gaze tracking session.
    /// </summary>
    /// <author>Agnese Pinto</author>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly GPService _gpService;
        private FrameworkElement _currentGazeElement;
        private string _focusedElementName;

        public GPCursor MyGazeCursor => _gpService.GazeCursor;
        public bool IsCursorVisible => _gpService.IsCursorVisible;

        /// <summary>
        /// Gets or sets the name of the UI element currently targeted by the gaze.
        /// </summary>
        public string FocusedElementName
        {
            get => _focusedElementName;
            set
            {
                _focusedElementName = value;
                OnPropertyChanged(nameof(FocusedElementName));
            }
        }

        /// <summary>
        /// Command to initiate the gaze tracking and open the form window.
        /// </summary>
        public ICommand StartCommand { get; }

        /// <summary>
        /// Command to initiate the gaze tracking and open the form window.
        /// </summary>
        public ICommand StartStressTestCommand { get; }

        /// <summary>
        /// Command to stop the service and shut down the application.
        /// </summary>
        public ICommand StopCommand { get; }

        /// <summary>
        /// Command to simulate a click (Enter) on the focus UI element.
        /// </summary>
        public ICommand PressEnterCommand { get; }

        /// <summary>
        /// Initialize a new instance of the MainViewModel and sets up service interactions.
        /// </summary>
        public MainViewModel()
        {
            _gpService = new GPService();
            _gpService.OnElementFocused += OnGazeFocusUpdate;

            PressEnterCommand = new RelayCommand(_ =>
            {
                if (_currentGazeElement != null)
                {
                    var handler = GazeHandlerFactory.GetHandler(_currentGazeElement.GetType());

                    var resultElement = handler.Execute(_currentGazeElement, _gpService);

                    _currentGazeElement = resultElement;
                    FocusedElementName = resultElement?.Name;
                }
            });

            StartCommand = new RelayCommand(_ =>
            {
                _gpService.Start();
                var form = new Form();

                var oldWindow = Application.Current.MainWindow;
                Application.Current.MainWindow = form;

                _gpService.UpdateWindowContext(form);
                form.DataContext = new FormViewModel(_gpService);
                form.Show();

                oldWindow.Close();
            });

            StartStressTestCommand = new RelayCommand(_ =>
            {
                _gpService.Start();
                var formWindow = new FormWindow();
                _gpService.UpdateWindowContext(formWindow);
                var formViewModel = new FormViewModel(_gpService);
                formWindow.DataContext = formViewModel;
                formWindow.Show();
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = formWindow;
            });

            StopCommand = new RelayCommand(_ =>
            {
                _gpService.Stop();
                Application.Current.Shutdown();
            });
        }

        private void OnGazeFocusUpdate(FrameworkElement element)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _currentGazeElement = element;
                FocusedElementName = element?.Name;

                if (element != null && element.Focusable)
                {
                    element.Focus();

                    if (element is ComboBoxItem item)
                    {
                        item.IsSelected = true; 
                    }
                }
            });
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}






//using System.ComponentModel;
//using System.Windows.Input;
//using Gaze_Point.Services;
//using Gaze_Point.GPModel.GPRecord;
//using System.Windows;
//using System.Windows.Controls;
//using Gaze_Point.GPModel.GPCursor;
//using System;
//using Gaze_Point.GPView;

//namespace Gaze_Point.GPViewModel
//{

//    /// <summary>
//    /// Coordinates the main application logic and handles navigation between the main screen and the form.
//    /// Manages high-level commands for starting and stopping the gaze tracking session.
//    /// </summary>
//    /// <author>Agnese Pinto</author>


//    public class MainViewModel : INotifyPropertyChanged
//    {
//        private readonly GPService _gpService;
//        private FrameworkElement _currentGazeElement;
//        private string _focusedElementName;

//        public GPCursor MyGazeCursor => _gpService.GazeCursor;
//        public bool IsCursorVisible => _gpService.IsCursorVisible;


//        /// <summary>
//        /// Gets or sets the name of the UI element currently targeted by the gaze.
//        /// </summary>
//        public string FocusedElementName
//        {
//            get => _focusedElementName;
//            set { _focusedElementName = value; OnPropertyChanged(nameof(FocusedElementName)); }
//        }


//        /// <summary>
//        /// Command to initiate the gaze tracking and open the demo window.
//        /// </summary>
//        public ICommand StartDemoCommand { get; }


//        /// <summary>
//        /// Command to initiate the gaze tracking and open the form window.
//        /// </summary>
//        public ICommand StartCommand { get; }


//        /// <summary>
//        /// Command to initiate the gaze tracking and open the form window.
//        /// </summary>
//        public ICommand StartStressTestCommand { get; }


//        /// <summary>
//        /// Command to stop the service and shut down the application.
//        /// </summary>
//        public ICommand StopCommand { get; }


//        /// <summary>
//        /// Command to simulate a click (Enter) on the focus UI element.
//        /// </summary>
//        public ICommand PressEnterCommand { get; }


//        /// <summary>
//        /// Initialize a new instance of the MainViewModel and sets up service interactions.
//        /// </summary>
//        public MainViewModel()
//        {
//            _gpService = new GPService();
//            _gpService.OnElementFocused += OnGazeFocusUpdate;

//            PressEnterCommand = new RelayCommand(_ =>
//            {
//                if (_currentGazeElement != null)
//                {
//                    if (_currentGazeElement is Button b)
//                    {
//                        b.Command?.Execute(b.CommandParameter);
//                    }
//                }
//            });

//            // In MainViewModel.cs, nel comando StartDemoCommand:
//            StartDemoCommand = new RelayCommand(async _ => // Nota: aggiunto async
//            {
//                _gpService.Start();
//                _gpService.UnsubscribeAllFromElementFocused();

//                var demo = new Demo();
//                Application.Current.MainWindow = demo;
//                var formViewModel = new FormViewModel(_gpService);
//                demo.DataContext = formViewModel;
//                demo.Show();

//                // ASPETTA 200ms che la finestra sia renderizzata prima di scansionare
//                await System.Threading.Tasks.Task.Delay(200);
//                _gpService.UpdateWindowContext(demo);
//            });


//            StartDemoCommand = new RelayCommand(_ =>
//            {
//                _gpService.Start();
//                _gpService.UnsubscribeAllFromElementFocused();

//                var demo = new Demo();

//                Application.Current.MainWindow = demo;

//                var formViewModel = new FormViewModel(_gpService);
//                demo.DataContext = formViewModel;

//                _gpService.UpdateWindowContext(demo);

//                demo.Show();

//            });


//            StartCommand = new RelayCommand(_ =>
//            {
//                _gpService.Start();

//                _gpService.UnsubscribeAllFromElementFocused();

//                var form = new Form();

//                _gpService.UpdateWindowContext(form);

//                var formViewModel = new FormViewModel(_gpService);
//                form.DataContext = formViewModel;
//                form.Show();

//                Application.Current.MainWindow.Close();
//                Application.Current.MainWindow = form;
//            });


//            StartStressTestCommand = new RelayCommand(_ =>
//            {
//                _gpService.Start();

//                _gpService.UnsubscribeAllFromElementFocused();

//                var formWindow = new FormWindow();

//                _gpService.UpdateWindowContext(formWindow);

//                var formViewModel = new FormViewModel(_gpService);
//                formWindow.DataContext = formViewModel;
//                formWindow.Show();

//                Application.Current.MainWindow.Close();
//                Application.Current.MainWindow = formWindow;
//            });


//            StopCommand = new RelayCommand(_ =>
//            {
//                _gpService.Stop();
//                Application.Current.Shutdown();
//            });
//        }

//        private void OnGazeFocusUpdate(FrameworkElement element)
//        {
//            Application.Current.Dispatcher.Invoke(() =>
//            {
//                _currentGazeElement = element;
//                FocusedElementName = element?.Name;
//            });
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }
//}





