using System;
using System.Windows;
using System.Windows.Threading;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPDwellManager
    {
        private FrameworkElement _currentElement;
        private DateTime _focusStartTime;

        // Tempo necessario per considerare un elemento "fissato" (es. 500ms)
        private readonly TimeSpan _dwellTime = TimeSpan.FromMilliseconds(500);

        // Evento che scatta quando un elemento è stato fissato per il tempo stabilito
        public event Action<FrameworkElement> OnElementFocused;

        public void Update(FrameworkElement elementUnderGaze)
        {
            // CASO 1: Non stiamo guardando nulla di rilevante
            if (elementUnderGaze == null)
            {
                _currentElement = null;
                return;
            }

            // CASO 2: Lo sguardo si è spostato su un NUOVO elemento
            if (elementUnderGaze != _currentElement)
            {
                _currentElement = elementUnderGaze;
                _focusStartTime = DateTime.Now; // Facciamo ripartire il cronometro
                return;
            }

            // CASO 3: Stiamo continuando a guardare lo stesso elemento
            var fixationTime = DateTime.Now - _focusStartTime;
            if (fixationTime >= _dwellTime)
            {
                // Se il tempo trascorso supera la soglia, lanciamo l'evento di "Focus avvenuto"
                OnElementFocused?.Invoke(_currentElement);

                // Resettiamo il timer per lanciare l'evento una sola volta per fissazione
                _focusStartTime = DateTime.MaxValue;
            }
        }
    }
}

