using SharedLibrary;
using System.Windows;

namespace PizzaEcki.Pages
{
    public partial class DriverDialog : Window
    {
        public string DriverName { get; private set; }
        public string PhoneNumber { get; private set; }

        public DriverDialog(Driver selectedDriver = null) // Optionaler Parameter
        {
            InitializeComponent();

            // Wenn ein Fahrer übergeben wird, füllen Sie die Felder mit den vorhandenen Daten
            if (selectedDriver != null)
            {
                NameTextBox.Text = selectedDriver.Name;
                PhoneNumberTextBox.Text = selectedDriver.PhoneNumber;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DriverName = NameTextBox.Text;
            PhoneNumber = PhoneNumberTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

}
