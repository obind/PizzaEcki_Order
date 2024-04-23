﻿using System;
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
                if (selectedPizzasListBox.Items.Count < 6) // Erlaube bis zu 6 Einträge
                {
                    Dish selectedItem = PizzaComboBox.SelectedItem as Dish;
                    if (selectedItem != null)
                    {
                        selectedPizzas.Add(selectedItem);
                        selectedPizzasListBox.Items.Add(selectedItem);

                        PizzaComboBox.Text = string.Empty; // Leere den Text in der ComboBox für eine neue Auswahl
                        UpdatePrice(); // Aktualisiere den Preis
                    }
                }
                else
                {
                    MessageBox.Show("Es können maximal 6 Pizzen ausgewählt werden.");
                }
                e.Handled = true; // Markiere das Ereignis als behandelt
                PizzaComboBox.Focus();
            }
        }

        private void SelectedPizzasListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && selectedPizzasListBox.SelectedItem != null)
            {
                Console.WriteLine("Selected Item Type: " + selectedPizzasListBox.SelectedItem.GetType().FullName);  // Debug Ausgabe

                Dish dishToRemove = selectedPizzasListBox.SelectedItem as Dish;
                if (dishToRemove != null)
                {
                    selectedPizzas.Remove(dishToRemove);
                    selectedPizzasListBox.Items.Remove(dishToRemove);
                    UpdatePrice();  // Methode zum Aktualisieren des Preises nach dem Entfernen
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
            double averagePrice = AveragePricePerPizza; // Berechnung des Durchschnittspreises

            // Optional: aktualisiere zusätzlich das `tempOrderItem`, wenn nötig
            tempOrderItem.Epreis = averagePrice;
        }


        public double AveragePricePerPizza
        {
            get
            {
                double totalPrice = SelectedPizzasPrices.Sum();
                int pizzaCount = selectedPizzas.Count; // Verwende die Liste direkt für eine genaue Zählung
                return pizzaCount > 0 ? totalPrice / pizzaCount : 0;
            }
        }



    }
}
