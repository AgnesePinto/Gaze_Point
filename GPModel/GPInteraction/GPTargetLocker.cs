using System;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;

namespace Gaze_Point.GPModel.GPInteraction
{
    public class GPTargetLocker
    {
        private readonly DispatcherTimer _timer;
        private bool _isLocked;
        private readonly double lockTime;

        // Proprietà per sapere se il puntamento oculare è sospeso
        public bool IsLocked => _isLocked;

        // Evento che avvisa il GPService quando il tempo è scaduto
        public event Action OnLockExpired;

        public GPTargetLocker()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/DataSettings.json")
                    .Build();

                lockTime = double.Parse(config["Interaction:LockTime"]);
            }
            catch
            {
                // Fallback in caso di errore caricamento file
                lockTime = 3000.0;
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(lockTime);
            _timer.Tick += (s, e) => ReleaseLock();
        }

        public void Activate()
        {
            _isLocked = true;
            _timer.Stop(); // Reset se era già attivo
            _timer.Start();
        }

        private void ReleaseLock()
        {
            _timer.Stop();
            _isLocked = false;
            OnLockExpired?.Invoke(); // Avvisa il Service che può tornare a "decidere"
        }
    }
}

