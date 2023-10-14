using PizzaEcki.Database;
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
    /// Interaktionslogik für TableView.xaml
    /// </summary>
    public partial class TableView : Window
    {

        private readonly DatabaseManager _databaseManager; // Stelle sicher, dass du einen Manager oder eine Datenbankverbindung hast

        public TableView()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();

            LoadTableNames();
        }

        List<string> tableNames = new List<string>();


        private void LoadTableNames()
        {
            tableNames = _databaseManager.GetTableNames();
            tablesComboBox.ItemsSource = tableNames;
        }

        private void TablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tablesComboBox.SelectedItem is string selectedTable)
            {
                var tableData = _databaseManager.GetTableData(selectedTable);
                dataGrid.ItemsSource = tableData.DefaultView;
            }
        }
    }
}
