﻿using Microsoft.AspNetCore.SignalR.Client;
using PizzaEcki.Models;
using SharedLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaEcki.Services
{
    public class SignalRService
    {
        private HubConnection hubConnection;

        public SignalRService()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7166/pizzaHub") // Update the URL to match your server's address
                .Build();

        }

        public async Task StartConnectionAsync()
        {
            await hubConnection.StartAsync();
        }

        public async Task SendOrderItemsAsync(List<OrderItem> orderItems)
        {
            await hubConnection.InvokeAsync("SendOrderItems", orderItems);
        }
    }
}
