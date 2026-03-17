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

            // Sottoscrizione locale per l'evidenziazione nella MainWindow
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

            StartCommand = new RelayCommand(_ => {
                _gpService.Start();

                // 1. PULIZIA TOTALE DEI VECCHI LISTENERS
                // Questo rimuove il metodo OnGazeFocusUpdate e qualsiasi altra sottoscrizione pendente
                _gpService.ClearFocusedElementSubscriptions();

                var formWindow = new FormWindow();

                // 2. Crea il nuovo ViewModel come unica sottoscrizione nel sevice
                var formViewModel = new FormViewModel(_gpService);

                formWindow.DataContext = formViewModel;
                formWindow.Show();

                // 3. Chiude la finestra corrente e aggiorna il riferimento globale
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = formWindow;
            });

            StopCommand = new RelayCommand(_ => {
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


