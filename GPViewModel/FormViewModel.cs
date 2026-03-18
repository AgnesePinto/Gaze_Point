using Gaze_Point.GPModel.GPCursor;
using Gaze_Point.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Gaze_Point.GPViewModel
{
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

        private string _nome;
        public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(nameof(Nome)); } }

        private string _cognome;
        public string Cognome { get => _cognome; set { _cognome = value; OnPropertyChanged(nameof(Cognome)); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(nameof(Email)); } }

        private string _telefono;
        public string Telefono { get => _telefono; set { _telefono = value; OnPropertyChanged(nameof(Telefono)); } }

        private string _cap;
        public string CAP { get => _cap; set { _cap = value; OnPropertyChanged(nameof(CAP)); } }

        public FormViewModel(GPService existingService)
        {
            _gpService = existingService;

            _gpService.OnElementFocused += (element) =>
            {
                System.Diagnostics.Debug.WriteLine($"Sguardo su elemento: {element?.Name ?? "Senza Nome"} (Tipo: {element?.GetType().Name})");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentGazeElement = element;
                    FocusedElementName = element?.Name;

                    // FEEDBACK PER COMBOBOXITEM
                    if (element is ComboBoxItem item)
                    {
                        // 'IsHighlighted' è la proprietà interna di WPF per l'elemento 
                        // che l'utente sta "puntando" nella lista (senza averlo ancora cliccato).
                        // Lo usiamo per attivare il feedback visivo.
                        item.IsSelected = false; // Non lo selezioniamo ancora definitivamente
                        item.Focus();          // Diamo il focus per attivare gli stili visivi
                    }
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
                    // 1. Gestione TextBox
                    if (_currentGazeElement is TextBox tb)
                    {
                        tb.Focus();
                        tb.CaretIndex = tb.Text.Length;
                    }
                    // 2. NOVITÀ: Gestione ComboBox (Apertura tendina)
                    else if (_currentGazeElement is ComboBox cb)
                    {
                        cb.IsDropDownOpen = !cb.IsDropDownOpen;
                        cb.Focus();
                        // NOVITÀ: Se la tendina si è aperta, chiediamo al servizio di 
                        // scansionare immediatamente i nuovi elementi (ComboBoxItem)
                        if (cb.IsDropDownOpen)
                        {
                            _gpService.RefreshInteractionTargets();
                        }
                    }
                    // 3. NOVITÀ: Gestione ComboBoxItem (Selezione elemento nella tendina)
                    else if (_currentGazeElement is ComboBoxItem item)
                    {
                        item.IsSelected = true;
                        // Cerchiamo la ComboBox "padre" per chiudere la tendina dopo la selezione
                        var parentCombo = ItemsControl.ItemsControlFromItemContainer(item) as ComboBox;
                        if (parentCombo != null)
                        {
                            parentCombo.IsDropDownOpen = false;
                            parentCombo.Focus();
                            // Puliamo la cache dagli elementi della tendina ormai chiusa
                            _gpService.RefreshInteractionTargets();
                        }
                    }
                    else
                    {
                        // 4. Gestione standard per altri elementi (Button, CheckBox, RadioButton)
                        _currentGazeElement.Focus();

                        if (_currentGazeElement is Button b)
                        {
                            b.Command?.Execute(b.CommandParameter);
                        }
                        else if (_currentGazeElement is RadioButton rb)
                        {
                            rb.IsChecked = true;
                        }
                        else if (_currentGazeElement is CheckBox chk)
                        {
                            chk.IsChecked = !chk.IsChecked;
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
