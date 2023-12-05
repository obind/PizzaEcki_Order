using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using SharedLibrary;
using System.Text;
using System.Configuration;
using System.Windows;
using System;
using System.Xml.Linq;

public class ApiService
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _apiBaseUrl;

    public ApiService()
    {
        try
        {
            // Lade die XML-Datei
            XDocument doc = XDocument.Load("KitchenClientSettings.xml");
            var server = doc.Root.Element("ConnectionSettings").Element("Server").Value;
            var port = doc.Root.Element("ConnectionSettings").Element("Port").Value;

            _apiBaseUrl = $"http://{server}:{port}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Laden der Konfigurationsdatei: {ex.Message}");
            _apiBaseUrl = "http://localhost:5057"; // Fallback-Wert
        }
    }
    public async Task<HttpResponseMessage> CheckConnectionAsync()
    {
        return await _httpClient.GetAsync($"{_apiBaseUrl}/healthcheck"); // Ein einfacher Endpunkt auf dem Server, der eine schnelle Antwort gibt
    }
    public async Task<List<Order>> GetUnassignedOrdersAsync()
    {
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/unassignedOrders");
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<Order>>(responseContent);
        return orders;
    }

    // In deinem ApiService
    public async Task<List<Driver>> GetAllDriversAsync()
    {
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/drivers");
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        var drivers = JsonSerializer.Deserialize<List<Driver>>(responseContent);
        return drivers;
    }

    public async Task SaveOrderAssignmentAsync(string orderId, int driverId, double price)
    {
        var requestBody = new
        {
            OrderId = orderId,
            DriverId = driverId,
            Price = price
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/orderAssignments", requestContent);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Customer> GetCustomerByPhoneNumberAsync(string phoneNumber)
    {
        // Überprüfen, ob die Telefonnummer gültig ist
        if (string.IsNullOrEmpty(phoneNumber))
        {
            MessageBox.Show("Keine Telefonnummer angegeben.");
            return null;
        }

        try
        {
            // Erstellen der URL mit dem Query-Parameter für die Telefonnummer
            var url = $"{_apiBaseUrl}/GetCustomer?phoneNumber={phoneNumber}";

            // Senden der GET-Anfrage
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Lesen und Deserialisieren der Antwort
            string responseContent = await response.Content.ReadAsStringAsync();
            Customer customer = JsonSerializer.Deserialize<Customer>(responseContent);

            return customer;
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Fehler bei der HTTP-Anfrage: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Laden des Kunden: {ex.Message}");
            return null;
        }
    }



}
