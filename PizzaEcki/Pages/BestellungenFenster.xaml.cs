using SharedLibrary;
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
    /// Interaktionslogik für BestellungenFenster.xaml
    /// </summary>
    public partial class BestellungenFenster : Window
    {
        public BestellungenFenster(List<SharedLibrary.Order> orders)
        {
            InitializeComponent();
            // Hier wird die Liste den Items der ListView zugewiesen oder ähnliches
            BestellungenListView.ItemsSource = orders;
        }

        private void LoadOrders(string bestellungsTyp)
        {
            // Hier würdest du deine Datenquelle abfragen, um die Bestellungen zu erhalten
            // Die folgende Zeile ist nur ein Platzhalter für die tatsächliche Datenabfrage
            //var bestellungen = BestellungenDatenService.GetBestellungenNachTyp(bestellungsTyp);

            // Setze die erhaltenen Bestellungen als ItemsSource für deine ListView
           // BestellungenListView.ItemsSource = bestellungen;
        }

      

    }
}
