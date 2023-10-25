using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PizzaEcki.Services
{
    public class BonNumberResetter
    {
        private DispatcherTimer resetBonNumberTimer;

        public event Action BonNumberReset;

        public BonNumberResetter()
        {
            SetupResetBonNumberTimer();
        }

        private void SetupResetBonNumberTimer()
        {
            resetBonNumberTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)  // Prüfe jede Minute
            };
            resetBonNumberTimer.Tick += ResetBonNumberTimer_Tick;
            resetBonNumberTimer.Start();
        }

        private void ResetBonNumberTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay < TimeSpan.FromMinutes(1))
            {
                // Es ist um oder kurz nach Mitternacht, setze die Bon-Nummer zurück
                BonNumberReset?.Invoke();
            }
        }
    }
}
