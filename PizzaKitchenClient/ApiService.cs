using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using SharedLibrary;

public class ApiService
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _apiBaseUrl;

    public ApiService()
    {
        _apiBaseUrl = "http://localhost:5062";
    }

    public async Task<List<Order>> GetUnassignedOrdersAsync()
    {
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/unassignedOrders");
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<Order>>(responseContent);
        return orders;
    }
}
