using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PizzaEcki
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            string mutexName = "OrderMate";

            bool isNewInstance;
            mutex = new Mutex(true, mutexName, out isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("Es läuft bereits eine Instanz dieser Anwendung.");
                Environment.Exit(0);
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
