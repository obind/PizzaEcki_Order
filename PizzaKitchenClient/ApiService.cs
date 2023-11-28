using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using SharedLibrary;
using System.Text;
using System.Configuration;

public class ApiService
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _apiBaseUrl;

    public ApiService()
    {
        // Lade die Basis-URL aus der Konfigurationsdatei
        _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:5062";
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

}
