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

        public string FocusedElementName
        {
            get => _focusedElementName;
            set { _focusedElementName = value; OnPropertyChanged(nameof(FocusedElementName)); }
        }

        // Proprietà per i dati (Nome e Cognome)
        public string Nome { get; set; }
        public string Cognome { get; set; }

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

            // 3. Logica del tasto INVIO
            PressEnterCommand = new RelayCommand(_ =>
            {
                if (_currentGazeElement != null)
                {
                    // Se l'elemento che sto guardando è una TextBox
                    if (_currentGazeElement is TextBox tb)
                    {
                        tb.Focus(); // Attiva il cursore per scrivere
                        tb.CaretIndex = tb.Text.Length; // Metti il cursore alla fine del testo
                    }
                    else if (_currentGazeElement is RadioButton rb)
                    {
                        rb.IsChecked = true;
                    }
                    else if (_currentGazeElement is CheckBox cb)
                    {
                        cb.IsChecked = true;
                    }
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
