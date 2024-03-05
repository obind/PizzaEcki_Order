using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PizzaEcki.Pages
{
    /// <summary>
    /// Interaktionslogik für SetPasswordDialog.xaml
    /// </summary>
    public partial class SetPasswordDialog : Window
    {
        public SetPasswordDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveEncryptedPassword(NewPasswordInput.Password);
            MessageBox.Show("Passwort wurde gesetzt.");
            this.DialogResult = true;
        }

        private void SaveEncryptedPassword(string password)
        {
            string encryptedPassword = EncryptPassword(password);
            Properties.Settings.Default.EncryptedPassword = encryptedPassword;
            Properties.Settings.Default.Save(); // Speichern der Änderungen
        }
        private string EncryptPassword(string password)
        {
            // Eine einfache Verschlüsselungsmethode (nur als Beispiel, für echte Anwendungen stärkere Methoden verwenden)
            // Ersetzen Sie dies durch Ihre bevorzugte Verschlüsselungsmethode
            byte[] data = Encoding.UTF8.GetBytes(password);
            // Verwenden Sie hier Ihre Verschlüsselungslogik
            return Convert.ToBase64String(data);
        }
    }
}
