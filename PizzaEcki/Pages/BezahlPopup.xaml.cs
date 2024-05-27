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
    /// Interaktionslogik für BezahlPopup.xaml
    /// </summary>
    public partial class BezahlPopup : Window
    {
        public string SelectedPaymentMethod { get; private set; }

        public BezahlPopup()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPaymentMethod = ((ComboBoxItem)BezahlCombobox.SelectedItem).Content.ToString();
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    BezahlCombobox.SelectedIndex = 1; // Barzahlung
                    break;
                case Key.D2:
                    BezahlCombobox.SelectedIndex = 2; // Kartenzahlung
                    break;
                case Key.D3:
                    BezahlCombobox.SelectedIndex = 3; // PayPal
                    break;
            }
        }
    }
}
