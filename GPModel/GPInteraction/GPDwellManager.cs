using System;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPDwellManager
    {
        private FrameworkElement _currentElement;
        private DateTime _focusStartTime;

        // Tempo necessario per considerare un elemento "fissato" (es. 300ms)
        private readonly TimeSpan _dwellTime;

        // Evento che scatta quando un elemento è stato fissato per il tempo stabilito
        public event Action<FrameworkElement> OnElementFocused;

        public GPDwellManager()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                int ms = int.Parse(config["Interaction:DwellTimeMs"]);
                _dwellTime = TimeSpan.FromMilliseconds(ms);
            }
            catch
            {
                _dwellTime = TimeSpan.FromMilliseconds(300);
            }
        }

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

