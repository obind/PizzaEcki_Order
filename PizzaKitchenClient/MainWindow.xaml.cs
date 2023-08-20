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
            .WithUrl("https://localhost:7166/pizzaHub") // Update the URL to match your server's address
            .Build();


            hubConnection.On<List<SharedLibrary.OrderItem>>("ReceiveOrderItems", (orderItems) =>
            {
                // Setzen Sie hier einen Breakpoint
                // Verarbeiten Sie die Liste der OrderItem-Objekte und aktualisieren Sie die UI
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in orderItems)
                    {
                        // Beispiel: Fügen Sie jedes Gericht zu einer Liste in der UI hinzu
                        DishList.Items.Add(item.Gericht + ", " + item.Extras + ", " + item.Menge);
                    }
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
