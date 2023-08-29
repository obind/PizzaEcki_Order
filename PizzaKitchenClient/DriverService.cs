using System.Text.Json;
using SharedLibrary;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PizzaKitchenClient
{
    public class DriverService
    {
        private readonly string _apiUrl;

        public DriverService(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public async Task<List<Driver>> GetDriversAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(_apiUrl + "/api/drivers");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    List<Driver> drivers = JsonSerializer.Deserialize<List<Driver>>(content);
                    return drivers;
                }
                else
                {
                    // Fehlerbehandlung
                    return null;
                }
            }
        }
    }
}
