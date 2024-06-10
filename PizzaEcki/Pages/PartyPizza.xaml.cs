using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
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
            PizzaComboBox.Focus();
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

        public string DescriptionOfSelectedPizzas
        {
            get
            {
                return "Party Pizza (" + string.Join(", ", selectedPizzas.Select(p => p.Id)) + ")";
            }
        }

        private void PizzaComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
          
                
                    // Wenn die ComboBox leer ist, zeige "OK" und bereite vor zum Schließen
                    if (PizzaComboBox.Text == "")
                    {
                        PizzaComboBox.Text = "OK";
                     
                    }
                    else if (PizzaComboBox.Text == "OK")
                    {
                        // Wenn "OK" schon angezeigt wird, schließe das Fenster
                        ConfirmAndClose();
                    }
                    else
                    {
                        // Hole das ausgewählte Pizza-Objekt
                        Dish selectedItem = PizzaComboBox.SelectedItem as Dish;
                        if (selectedItem != null)
                        {
                            // Prüfe, ob die maximale Anzahl von Pizzen bereits erreicht ist
                            if (selectedPizzas.Count >= 6)
                            {
                                MessageBox.Show("Es können maximal 6 Pizzen ausgewählt werden.");
                            }
                            else
                            {
                                // Füge die ausgewählte Pizza hinzu und aktualisiere die UI
                                selectedPizzas.Add(selectedItem);
                                selectedPizzasListBox.Items.Add(selectedItem);
                                PizzaComboBox.Text = string.Empty; // Leere die ComboBox

                                UpdatePrice(); // Aktualisiere den Preis
                            }
                        }
                        else
                        {
                            MessageBox.Show("Bitte wähle eine gültige Pizza aus.");
                        }

                        e.Handled = true; // Verhindere weitere Event-Handler
                    }
                e.Handled = true; // Verhindert weitere Handler
                
                PizzaComboBox.Focus(); // Setzt den Fokus zurück auf die ComboBox
            }
        }

        private void ConfirmAndClose()
        {
            if (selectedPizzas.Count > 0)
            {
                this.DialogResult = true; // Setze DialogResult auf true, um anzuzeigen, dass alles korrekt abgeschlossen wurde
                this.Close(); // Schließe das Fenster
            }
            else
            {
                this.DialogResult = false;
          
            }
        }


        private void SelectedPizzasListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && selectedPizzasListBox.SelectedItem != null)
            {
                Console.WriteLine("Selected Item Type: " + selectedPizzasListBox.SelectedItem.GetType().FullName);

                Dish dishToRemove = selectedPizzasListBox.SelectedItem as Dish;
                if (dishToRemove != null)
                {
                    selectedPizzas.Remove(dishToRemove);
                    selectedPizzasListBox.Items.Remove(dishToRemove);
                    UpdatePrice();
                }
                else
                {
                    Console.WriteLine("Fehler: Die Umwandlung des selektierten Items in 'Dish' ist fehlgeschlagen.");
                }
            }
        }



        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void UpdatePrice()
        {
            double averagePrice = AveragePricePerPizza;
            tempOrderItem.Epreis = averagePrice;
        }

        public double AveragePricePerPizza
        {
            get
            {
                double totalPrice = SelectedPizzasPrices.Sum();
                int pizzaCount = selectedPizzas.Count;
                return pizzaCount > 0 ? totalPrice / pizzaCount : 0;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F7)
            {
                RemoveLastPizza();
            }
        }

        private void RemoveLastPizza()
        {
            if (selectedPizzas.Any())
            {
                var lastPizza = selectedPizzas.Last();
                selectedPizzas.Remove(lastPizza);
                selectedPizzasListBox.Items.Remove(lastPizza);
                UpdatePrice();
                MessageBox.Show($"{lastPizza.Name} wurde entfernt.", "Pizza entfernt", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Keine Pizzen zum Entfernen vorhanden.", "Aktion nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
