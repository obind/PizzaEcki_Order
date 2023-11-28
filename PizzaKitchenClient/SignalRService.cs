using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Configuration; // Du musst das System.Configuration NuGet-Paket hinzufügen
using System.Diagnostics;
using System.Threading.Tasks;


namespace PizzaKitchenClient
{
    public class SignalRService
    {
        public HubConnection HubConnection { get; }

        public SignalRService()
        {
            // Lade die URL aus der Konfigurationsdatei
            string hubUrl = ConfigurationManager.AppSettings["SignalRHubUrl"];

            HubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            HubConnection.Closed += async (error) =>
            {
                // Logge das Trennungsereignis
                Debug.WriteLine($"Verbindung getrennt: {error?.Message}");

                // Versuche die Verbindung wiederherzustellen
                await Task.Delay(new Random().Next(0, 5) * 1000);
                try
                {
                    await HubConnection.StartAsync();
                }
                catch (Exception ex)
                {
                    // Logge Fehler beim Wiederverbindungsversuch
                    Debug.WriteLine($"Fehler beim Wiederverbinden: {ex.Message}");
                }
            };

            HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                // Handle the message
                Debug.WriteLine($"Geilo es  Geht {user}: {message}");
                // Hier könntest du auch eine Event-Auslösung haben, um die UI zu aktualisieren
            });
        }

        // ... Rest der Klasse
    }
}
