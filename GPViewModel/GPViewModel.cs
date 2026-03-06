using System.ComponentModel;
using System.Windows.Input;
using Gaze_Point.Services;
using Gaze_Point.GPModel.GPRecord;
using System.Windows;

namespace Gaze_Point.GPViewModel
{
    public class MainViewModel : INotifyPropertyChanged         // Implementa l'interfaccia Notify per comunicare le modifiche alla view
    {
        private readonly GPService _gpService;
        private string _status;

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }         // OnPropertyChanged lancia l'allarme ogni volta che una proprietà cambia
        }

        // Comandi per i bottoni della UI
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public MainViewModel()
        {
            _gpService = new GPService();           // Istanza del servizio che attiva l'eye tracker
            Status = "Disconnesso";

            // Colleghiamo i comandi
            StartCommand = new RelayCommand(_ => {
                _gpService.Start();
                Status = "Tracking Attivo";
            });

            StopCommand = new RelayCommand(_ => {
                _gpService.Stop();
                Status = "Tracking Fermato";
            });

            // Se vuoi mostrare le coordinate in tempo reale nella UI:
            _gpService.OnDataReceived += (data) => {
                // Esempio: aggiorna una label con le coordinate
                // Status = $"X: {data.BPOGX:F2} Y: {data.BPOGY:F2}";
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

