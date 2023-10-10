﻿using Microsoft.AspNetCore.SignalR.Client;
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

namespace PizzaKitchenClient
{
    public partial class MainWindow : Window
    {
        private HubConnection hubConnection;
        private readonly HttpClient httpClient = new HttpClient();
        public ObservableCollection<Driver> Drivers { get; set; }
        private const string DriversApiUrl = "https://localhost:7166/api/drivers"; // Ersetze die URL durch die tatsächliche Adresse deiner API
        DatabaseManager dbManager = new DatabaseManager();

        public MainWindow()
        {
            InitializeComponent();
            InitializeHubConnection();
            LoadDrivers();
        }

        private void LoadDrivers()
        {
            List<Driver> driversFromDb = dbManager.GetAllDrivers();
            Drivers = new ObservableCollection<Driver>(driversFromDb);
            DriversComboBox.ItemsSource = Drivers;
        }


        private void InitializeHubConnection()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7166/pizzaHub")
                .Build();

            hubConnection.On<Order>("ReceiveOrder", (order) =>
            {
                Dispatcher.Invoke(() =>
                {
                    OrdersList.Items.Add(order);
                });
            });

            StartHubConnection();
        }

        private async Task StartHubConnection()
        {
            try
            {
                await hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                // Handle connection error
                MessageBox.Show(ex.Message);
            }
        }

        private void DriversComboBox_Loaded(object sender, RoutedEventArgs e)
        {

            List<Driver> driversFromDb = dbManager.GetAllDrivers();
            Drivers = new ObservableCollection<Driver>(driversFromDb);
            DriversComboBox.ItemsSource = driversFromDb;
        }

        private void OnAssignButtonClicked(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is Order selectedOrder && DriversComboBox.SelectedItem is Driver selectedDriver)
            {
                // Berechne den Gesamtpreis der Bestellung
                double orderPrice = selectedOrder.OrderItems.Sum(item => item.Gesamt);

                // Speichere die Zuordnung
                dbManager.SaveOrderAssignment(selectedOrder.BonNumber, selectedDriver.Id, orderPrice);

                // Entferne die Bestellung aus der Liste
                OrdersList.Items.Remove(selectedOrder);
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Bestellung und einen Fahrer aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
