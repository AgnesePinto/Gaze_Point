using System.ComponentModel;
using System.Windows.Input;
using Gaze_Point.Services;
using Gaze_Point.GPModel.GPRecord;
using System.Windows;
using System.Windows.Controls;
using Gaze_Point.GPModel.GPCursor;
using System;

namespace Gaze_Point.GPViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly GPService _gpService;

        public GPCursor MyGazeCursor => _gpService.GazeCursor;
        public bool IsCursorVisible => _gpService.IsCursorVisible;

        private FrameworkElement _currentGazeElement;
        private string _focusedElementName;

        public string FocusedElementName
        {
            get => _focusedElementName;
            set { _focusedElementName = value; OnPropertyChanged(nameof(FocusedElementName)); }
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PressEnterCommand { get; }

        public MainViewModel()
        {
            _gpService = new GPService();
            _gpService.OnElementFocused += OnGazeFocusUpdate;

            PressEnterCommand = new RelayCommand(_ =>
            {
                if (_currentGazeElement != null)
                {
                    if (_currentGazeElement is Button b)
                    {
                        b.Command?.Execute(b.CommandParameter);
                    }
                }
            });

            StartCommand = new RelayCommand(_ =>
            {
                _gpService.Start();

                var formWindow = new FormWindow();

                // LOGICA IBRIDA: Reset immediato e scansione della nuova finestra
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

        // Metodo per gestire l'aggiornamento del focus
        private void OnGazeFocusUpdate(FrameworkElement element)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _currentGazeElement = element;
                FocusedElementName = element?.Name;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}





