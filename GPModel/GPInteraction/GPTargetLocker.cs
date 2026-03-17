using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPTargetLocker
    {
        private readonly DispatcherTimer _timer;
        private bool _isLocked;
        private readonly double _lockTime;
        private readonly List<Point> _collectedPoints = new List<Point>();

        public bool IsLocked => _isLocked;

        // L'evento restituisce la lista di punti accumulati al termine del blocco
        public event Action<List<Point>> OnLockExpired;

        public GPTargetLocker()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                _lockTime = double.Parse(config["Interaction:LockTime"]);
            }
            catch
            {
                _lockTime = 1500.0;
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_lockTime);
            _timer.Tick += (s, e) => ReleaseLock();
        }

        public void Activate()
        {
            _collectedPoints.Clear(); // Reset automatico dei punti ad ogni nuova attivazione
            _isLocked = true;
            _timer.Stop();
            _timer.Start();
        }

        /// <summary>
        /// Metodo chiamato da GPService per passare i dati durante il blocco.
        /// Gestisce internamente il filtro di validità (BPOGV == 1).
        /// </summary>
        public void ProcessPoint(double x, double y, int bpogv)
        {
            if (!_isLocked) return;

            // Filtro: scartiamo i dati di "riparazione" o invalidi (es. blink)
            if (bpogv == 1)
            {
                _collectedPoints.Add(new Point(x, y));
            }
        }

        private void ReleaseLock()
        {
            _timer.Stop();
            _isLocked = false;

            // Inviamo una copia della lista corretta aggiornata per evitare problemi di riferimento
            OnLockExpired?.Invoke(new List<Point>(_collectedPoints));

            _collectedPoints.Clear();
        }
    }
}

