using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaKitchenClient
{
    public class SignalRService
    {
        public HubConnection HubConnection { get; }

        public SignalRService()
        {
            HubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5062/pizzaHub")
                .Build();

            HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                // Handle the message
                Debug.WriteLine($"Message received from {user}: {message}");
                // Hier könntest du auch eine Event-Auslösung haben, um die UI zu aktualisieren
            });
        }

        // ... Rest der Klasse
    }

}
