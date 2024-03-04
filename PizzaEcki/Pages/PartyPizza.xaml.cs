using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using PizzaEcki.Database;
using PizzaEcki.Models;
using SharedLibrary;

namespace PizzaEcki.Pages
{
    public partial class PartyPizza : Window
    {
        private List<Dish> selectedPizzas = new List<Dish>();
        public List<int> SelectedPizzaIds => selectedPizzas.Select(p => p.Id).ToList();
        public List<double> SelectedPizzasPrices => selectedPizzas.Select(p => p.Preis_XL).ToList();

        private DatabaseManager _databaseManager = new DatabaseManager(); // Stelle sicher, dass dies korrekt initialisiert wird
        private List<Dish> dishesList;
        private OrderItem tempOrderItem = new OrderItem();

        public PartyPizza()
        {
            InitializeComponent();
            InitializePizzaComboBox();
        }

        private void InitializePizzaComboBox()
        {

            dishesList = _databaseManager.GetAllDishes();

            var pizzaDishes = dishesList.Where(dish => dish.Kategorie == DishCategory.Pizza).ToList();

            PizzaComboBox.ItemsSource = pizzaDishes;
        }

        

        private void AddSelectedPizza(Dish selectedPizza)
        {
            if (selectedPizzas.Count >= 6)
            {
                MessageBox.Show("Es können maximal 6 Pizzen ausgewählt werden.");
                return;
            }

            selectedPizzas.Add(selectedPizza);
        }

        private void PizzaComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PizzaComboBox.SelectedItem == null)
            {
              
                tempOrderItem.Gericht = "";
                tempOrderItem.Nr = 0;
                return;
            }

            Dish selectedDish = (Dish)PizzaComboBox.SelectedItem;
            tempOrderItem.Gericht = selectedDish.Name.ToString();
            tempOrderItem.OrderItemId = selectedDish.Id;
        }


        private void PizzaComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (selectedPizzasListBox.Items.Count < 6) // Erlaube bis zu 6 Einträge
                {
                    var selectedItem = PizzaComboBox.SelectedItem as Dish;
                    if (selectedItem != null)
                    {
                        PizzaComboBox.IsDropDownOpen = false;

                        // Füge das ausgewählte Gericht der ListBox hinzu, ohne zu prüfen, ob es bereits vorhanden ist
                        selectedPizzas.Add(selectedItem);
                        selectedPizzasListBox.Items.Add(selectedItem.ToString());

                        PizzaComboBox.Text = string.Empty; // Leere den Text in der ComboBox für eine neue Auswahl

                        // Stelle den Fokus zurück zur ComboBox
                        PizzaComboBox.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Es können maximal 6 Pizzen ausgewählt werden.");
                }

                e.Handled = true; // Markiere das Ereignis als behandelt
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
