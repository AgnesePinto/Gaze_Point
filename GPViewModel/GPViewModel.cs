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

        public GPCursor MyGazeCursor => _gpService.GazeCursor;
        public bool IsCursorVisible => _gpService.IsCursorVisible;

        private FrameworkElement _currentGazeElement; // Memorizza l'oggetto fisico sotto lo sguardo
        private string _focusedElementName;           // Memorizza il nome dell'oggetto per lo XAML

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
                    //else if (_currentGazeElement is CheckBox cb)
                    //{
                    //    cb.IsChecked = !cb.IsChecked;
                    //}
                }
            });

            // Colleghiamo i comandi ai pulsanti
            StartCommand = new RelayCommand(_ => {
                _gpService.Start();
                var formWindow = new FormWindow();

                // 3. Crea il nuovo ViewModel passandogli il servizio ATTIVO
                var formViewModel = new FormViewModel(_gpService);

                // 4. Collega il ViewModel alla finestra e mostrala
                formWindow.DataContext = formViewModel;
                formWindow.Show();

                // 5. Chiude la finestra corrente (opzionale, puoi anche usare .Hide())
                Application.Current.MainWindow.Close();

                // Aggiorna il riferimento della MainWindow globale per l'Hit-Testing
                Application.Current.MainWindow = formWindow;
            });

            //StopCommand = new RelayCommand(_ => {
            //    _gpService.Stop();
            //    Status = "Tracking Fermato";
            //    FocusedElementName = null; // Reset dell'evidenziazione allo stop
            //});

            StopCommand = new RelayCommand(_ => {
                _gpService.Stop();
                Application.Current.Shutdown();
            });

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}


