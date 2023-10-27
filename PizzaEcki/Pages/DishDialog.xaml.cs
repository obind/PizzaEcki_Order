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
        public Dish Dish { get; private set; }

        public DishDialog(Dish dish = null)
        {
            InitializeComponent();
            Dish = dish ?? new Dish();
            PopulateFields();
        }

        private void PopulateFields()
        {
            NameTextBox.Text = Dish.Name;
          //  PriceTextBox.Text = Dish.Preis.ToString();
            CategoryComboBox.ItemsSource = Enum.GetValues(typeof(DishCategory)).Cast<DishCategory>();
            CategoryComboBox.SelectedItem = Dish.Kategorie;
            SizeComboBox.ItemsSource = DishSizeManager.CategorySizes[Dish.Kategorie];
            SizeComboBox.SelectedItem = Dish.Größe;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Dish.Name = NameTextBox.Text;
          //  Dish.Preis = double.Parse(PriceTextBox.Text);
            Dish.Kategorie = (DishCategory)CategoryComboBox.SelectedItem;
            Dish.Größe = SizeComboBox.SelectedItem.ToString();
            DialogResult = true;
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is DishCategory selectedCategory)
            {
                SizeComboBox.ItemsSource = DishSizeManager.CategorySizes[selectedCategory];
            }
        }

        private void CategoryComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryComboBox.ItemsSource = Enum.GetValues(typeof(DishCategory)).Cast<DishCategory>();
        }


    }
}
