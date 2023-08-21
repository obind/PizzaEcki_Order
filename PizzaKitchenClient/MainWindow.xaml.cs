using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using SharedLibrary;

namespace PizzaKitchenClient
{
    public partial class MainWindow : Window
    {
        private HubConnection hubConnection;

        public MainWindow()
        {
            InitializeComponent();
            InitializeHubConnection();
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
                    // Hier können Sie die Bestellung im UI anzeigen
                    // Zum Beispiel:
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
    }
}
