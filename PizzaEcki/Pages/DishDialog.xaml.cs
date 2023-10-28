using PizzaEcki.Database;
using PizzaEcki.Models;
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
    /// Interaktionslogik für DishDialog.xaml
    /// </summary>
    public partial class DishDialog : Window
    {
        DatabaseManager _dbManager = new DatabaseManager();
        public Dish Dish { get; private set; }

        public DishDialog(Dish dish = null)
        {
            InitializeComponent();
            Dish = dish ?? new Dish();
            PopulateFields();
        }

        private void PopulateFields()
        {
            IdTextBox.Text = Dish.Id.ToString();
            NameTextBox.Text = Dish.Name;
            PriceSTextBox.Text = Dish.Preis_S.ToString();
            PriceLTextBox.Text = Dish.Preis_L.ToString();
            PriceXLTextBox.Text = Dish.Preis_XL.ToString();
            CategoryComboBox.ItemsSource = Enum.GetValues(typeof(DishCategory)).Cast<DishCategory>();
            CategoryComboBox.SelectedItem = Dish.Kategorie;
          
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            if (int.TryParse(IdTextBox.Text, out int id))
            {
                // Überprüfe, ob die ID bereits existiert
                if (_dbManager.IsIdExists(id))
                {
                    MessageBox.Show("Diese ID existiert bereits. Bitte wähle eine andere.");
                    return;
                }
                Dish.Id = id;
            }
            else
            {
                MessageBox.Show("Bitte gib eine gültige ID ein.");
                return;
            }


            Dish.Name = NameTextBox.Text;

            if (double.TryParse(PriceSTextBox.Text, out double preisS))
            {
                Dish.Preis_S = preisS;
            }

            if (double.TryParse(PriceLTextBox.Text, out double preisL))
            {
                Dish.Preis_L = preisL;
            }

            if (double.TryParse(PriceXLTextBox.Text, out double preisXL))
            {
                Dish.Preis_XL = preisXL;
            }

            Dish.Kategorie = (DishCategory)CategoryComboBox.SelectedItem;

            // HappyHour

                // HappyHour
                Dish.HappyHour = (HappyHourCheckBox.IsChecked == true ? 1 : 0).ToString();

            // Gratis Beilage
            Dish.GratisBeilage = FreeSideCheckBox.IsChecked == true ? 1 : 0;

           

            // Steuersatz
            if (double.TryParse(TaxRateTextBox.Text, out double steuersatz))
            {
                Dish.Steuersatz = steuersatz;
            }

            DialogResult = true;
        }


        private void CategoryComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryComboBox.ItemsSource = Enum.GetValues(typeof(DishCategory)).Cast<DishCategory>();
        }
    }
}