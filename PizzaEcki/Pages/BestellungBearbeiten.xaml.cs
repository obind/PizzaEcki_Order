using SharedLibrary;
using System.Windows;
using PizzaEcki.Database;
using System;
using System.Threading.Tasks;


namespace PizzaEcki.Pages
{
    /// <summary>
    /// Interaktionslogik für BestellungBearbeiten.xaml
    /// </summary>
    public partial class BestellungBearbeiten : Window
    {
        public BestellungBearbeiten()
        {
            InitializeComponent();
        }


        private async void Speichern_Click(object sender, RoutedEventArgs e)
        {
            var orderToUpdate = this.DataContext as Order;
            if (orderToUpdate != null)
            {
                try
                {
                    // Erhalte eine Instanz von DatabaseManager und rufe UpdateOrderAsync auf
                    var databaseManager = new DatabaseManager(); // Du musst hier sicherstellen, dass die Instanz korrekt erstellt wird, vielleicht durch Dependency Injection oder eine Factory-Methode.
                    await databaseManager.UpdateOrderAsync(orderToUpdate);

                    // Feedback an den Benutzer geben, dass die Speicherung erfolgreich war
                    MessageBox.Show("Die Bestellung wurde erfolgreich aktualisiert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Schließe das Fenster, wenn du möchtest, dass es sich nach dem Speichern schließt
                    this.Close();
                }
                catch (Exception ex)
                {
                    // Zeige dem Benutzer eine Fehlermeldung, falls etwas schiefgeht
                    MessageBox.Show($"Ein Fehler ist aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Zeige eine Fehlermeldung, wenn keine Order im DataContext ist
                MessageBox.Show("Keine Bestellung zum Speichern gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

}
