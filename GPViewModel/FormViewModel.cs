using Gaze_Point.GPModel.GPCursor;
using Gaze_Point.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input; // Necessario per ICommand
using System.Windows.Controls; // Necessario per riconoscere la classe TextBox

namespace Gaze_Point.GPViewModel
{
    public class FormViewModel : INotifyPropertyChanged
    {
        private readonly GPService _gpService;
        private string _focusedElementName;
        private FrameworkElement _currentGazeElement; // 1. Memorizza l'elemento fisico sotto lo sguardo

        public GPCursor MyGazeCursor => _gpService.GazeCursor;
        public bool IsCursorVisible => _gpService.IsCursorVisible;


        public string FocusedElementName
        {
            get => _focusedElementName;
            set { _focusedElementName = value; OnPropertyChanged(nameof(FocusedElementName)); }
        }

        // Proprietà per i dati (Nome e Cognome)
        //public string Nome { get; set; }
        //public string Cognome { get; set; }
        //public string Email { get; set; }
        public ICommand StopCommand { get; }

        // 2. Comando per gestire la pressione del tasto INVIO
        public ICommand PressEnterCommand { get; }

        public FormViewModel(GPService existingService)
        {
            _gpService = existingService;

            // Aggiorniamo l'evidenziazione degli elementi sotto lo sguardo
            _gpService.OnElementFocused += (element) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentGazeElement = element; // Aggiorna l'elemento attivo per il comando INVIO
                    FocusedElementName = element?.Name; // Aggiorna l'estetica (bordo spesso)
                });
            };

            StopCommand = new RelayCommand(_ => {
                _gpService.Stop();
                Application.Current.Shutdown();
            });

            PressEnterCommand = new RelayCommand(_ =>
            {
                if (_currentGazeElement != null)
                {
                    // 1. Gestione specifica per TextBox
                    if (_currentGazeElement is TextBox tb)
                    {
                        tb.Focus();
                        tb.CaretIndex = tb.Text.Length;
                    }
                    else
                    {
                        // 2. Se premo Invio su QUALSIASI ALTRO elemento (Button, CheckBox, ecc.)
                        // Spostiamo il focus all'elemento corrente per "staccarlo" dalla vecchia TextBox
                        _currentGazeElement.Focus();

                        // 3. Eseguiamo la logica specifica dell'elemento
                        if (_currentGazeElement is Button b)
                        {
                            b.Command?.Execute(b.CommandParameter);
                        }
                        else if (_currentGazeElement is RadioButton rb)
                        {
                            rb.IsChecked = true;
                        }
                        else if (_currentGazeElement is CheckBox cb)
                        {
                            cb.IsChecked = !cb.IsChecked;
                        }
                    }
                }
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
