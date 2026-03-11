using System.ComponentModel;
using System.Windows.Input;
using Gaze_Point.Services;
using Gaze_Point.GPModel.GPRecord;
using System.Windows;
using System.Windows.Controls; // Necessario per gestire CheckBox e TextBox nel comando
using Gaze_Point.GPModel.GPCursor;

namespace Gaze_Point.GPViewModel
{
    // Collega i pulsanti della finestra alle azioni del codice
    public class MainViewModel : INotifyPropertyChanged         // Implementa l'interfaccia Notify per comunicare le modifiche alla view
    {
        private readonly GPService _gpService;
        private string _status;

        public GPCursor MyGazeCursor => _gpService.GazeCursor;

        private FrameworkElement _currentGazeElement; // Memorizza l'oggetto fisico sotto lo sguardo
        private string _focusedElementName;           // Memorizza il nome dell'oggetto per lo XAML

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }         // OnPropertyChanged lancia l'allarme ogni volta che una proprietà cambia
        }

        // Proprietà che comunica allo XAML quale elemento deve diventare azzurro
        public string FocusedElementName
        {
            get => _focusedElementName;
            set { _focusedElementName = value; OnPropertyChanged(nameof(FocusedElementName)); }
        }

        // Comandi per i bottoni della UI
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        // Comando per gestire la pressione del tasto INVIO
        public ICommand PressEnterCommand { get; }

        public MainViewModel()
        {
            _gpService = new GPService();           // Istanza del servizio che attiva l'eye tracker
            Status = "Disconnesso";

            _gpService.OnElementFocused += (element) =>
            {
                // Questo comando dice: "Esegui questa azione sul thread della UI"
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentGazeElement = element;       // Memorizza l'elemento per il tasto Invio
                    FocusedElementName = element?.Name;  // Dice allo XAML di cambiare colore

                    // Questo scrive nella finestra "Output" di Visual Studio (utile per te)
                    ////System.Diagnostics.Debug.WriteLine($"Sguardo su: {FocusedElementName}");
                    ////System.Diagnostics.Debug.WriteLine($"NOME RILEVATO: '{FocusedElementName}'");
                });
            };

            // Logica universale per il tasto INVIO
            PressEnterCommand = new RelayCommand(_ =>
            {
                // Verifichiamo se abbiamo un elemento "agganciato" dallo sguardo
                if (_currentGazeElement != null)
                {
                    if (_currentGazeElement is Button b)
                    {
                        // Eseguiamo il comando del bottone 
                        b.Command?.Execute(b.CommandParameter);
                    }
                    else if (_currentGazeElement is CheckBox cb)
                    {
                        cb.IsChecked = !cb.IsChecked;
                    }
                }
            });

            // Colleghiamo i comandi ai pulsanti
            StartCommand = new RelayCommand(_ => {
                _gpService.Start();
                Status = "Tracking Attivo";
            });

            StopCommand = new RelayCommand(_ => {
                _gpService.Stop();
                Status = "Tracking Fermato";
                FocusedElementName = null; // Reset dell'evidenziazione allo stop
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


