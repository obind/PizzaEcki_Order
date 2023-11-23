using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using SharedLibrary;
using System.Windows.Documents;
using PizzaEcki.Database;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace PizzaKitchenClient
{
    public partial class MainWindow : Window
    {
        private HubConnection hubConnection;

        private readonly SignalRService signalRService = new SignalRService();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await signalRService.HubConnection.StartAsync();
        }


        private async void ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5062/pizzaHub")
                .Build();

            hubConnection.On<string>("ReceiveOrder", (order) =>
            {
                // Update UI to show the order
                Dispatcher.Invoke(() =>
                {
                    // UI updates must be done on the UI thread
                    // AddOrderToList(order);
                });
            });

            try
            {
                await hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                // Handle connection errors
            }


        }
        

        private void DriversComboBox_Loaded(object sender, RoutedEventArgs e)
        {

            //List<Driver> driversFromDb = dbManager.GetAllDrivers();
            //Drivers = new ObservableCollection<Driver>(driversFromDb);
            //DriversComboBox.ItemsSource = driversFromDb;
        }

        private void OnAssignButtonClicked(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is Order selectedOrder && DriversComboBox.SelectedItem is Driver selectedDriver)
            {
                if (!selectedOrder.IsDelivery)
                {
                    MessageBox.Show("Fahrer können keine Abholungen übernehmen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    // Berechne den Gesamtpreis der Bestellung
                    double orderPrice = selectedOrder.OrderItems.Sum(item => item.Gesamt);

                    // Speichere die Zuordnung
                  //  dbManager.SaveOrderAssignment(selectedOrder.OrderId.ToString(), selectedDriver.Id, orderPrice);

                    // Entferne die Bestellung aus der Liste
                    OrdersList.Items.Remove(selectedOrder);

                    DriversComboBox.SelectedItem = null;
                }
            }
            else if (OrdersList.SelectedItem is Order anotherSelectedOrder && !anotherSelectedOrder.IsDelivery)
            {
            
            
                    // Entferne die Bestellung aus der Liste
                    OrdersList.Items.Remove(anotherSelectedOrder);
             
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Bestellung und einen Fahrer aus.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        


    }
}
