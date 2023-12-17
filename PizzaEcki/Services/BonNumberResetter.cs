using System;
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
                Interval = TimeSpan.FromDays(1) // Setze den Timer so ein, dass er einmal pro Tag tickt
            };
            resetBonNumberTimer.Tick += ResetBonNumberTimer_Tick;
            resetBonNumberTimer.Start();
        }

        private void ResetBonNumberTimer_Tick(object sender, EventArgs e)
        {
            // Da der Timer nur einmal pro Tag tickt, brauchen wir hier keine Zeitüberprüfung
            BonNumberReset?.Invoke();
        }
    }

}
