using Gaze_Point.GPModel.GPCursor;
using Gaze_Point.GPView;
using Gaze_Point.GPViewModel.Handlers;
using Gaze_Point.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Gaze_Point.GPViewModel
{

    /// <summary>
    /// Manages form interactions to decouple UI logic from gaze events.
    /// Maps framework elements to specific handlers for focus and action execution.
    /// </summary>
    /// <author>Agnese Pinto</author>
    

    public class FormViewModel : INotifyPropertyChanged
    {
        private readonly GPService _gpService;
        private string _focusedElementName;
        private FrameworkElement _currentGazeElement;

        public GPCursor MyGazeCursor => _gpService.GazeCursor;
        public bool IsCursorVisible => _gpService.IsCursorVisible;

        public string FocusedElementName
        {
            get => _focusedElementName;
            set { _focusedElementName = value; OnPropertyChanged(nameof(FocusedElementName)); }
        }

        public ICommand StopCommand { get; }
        public ICommand PressEnterCommand { get; }
        public ICommand ReturnToMainCommand { get; }

        public FormViewModel(GPService existingService)
        {
            _gpService = existingService;


            _gpService.OnElementFocused += (element) =>
            {
                //System.Diagnostics.Debug.WriteLine($"Sguardo su elemento: {element?.Name ?? "Senza Nome"} (Tipo: {element?.GetType().Name})");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentGazeElement = element;
                    FocusedElementName = element?.Name;

                    if (element is ComboBoxItem item)
                    {
                        item.IsSelected = false; 
                        item.Focus();        
                    }
                });
            };


            StopCommand = new RelayCommand(_ => {
                _gpService.Stop();
                Application.Current.Shutdown();
            });


            ReturnToMainCommand = new RelayCommand(_ =>
            {
                _gpService.Stop();
                var mainWindow = new MainWindow();
                _gpService.UpdateWindowContext(mainWindow);
                var oldWindow = Application.Current.MainWindow;
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                oldWindow?.Close();
                _gpService.Start();        
            });


            PressEnterCommand = new RelayCommand(_ =>
            {
                if (_currentGazeElement == null) return;

                var handler = GazeHandlerFactory.GetHandler(_currentGazeElement.GetType());

                var resultElement = handler.Execute(_currentGazeElement, _gpService);

                _currentGazeElement = resultElement;
                FocusedElementName = resultElement?.Name;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void SetFocusedElement(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                FocusedElementName = name;
            }
        }
    }
}
