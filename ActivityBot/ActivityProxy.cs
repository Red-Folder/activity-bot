using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ActivityBot
{
    public class ActivityProxy
    {
        private readonly HttpClient _httpClient;
        
        public ActivityProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAwaiting()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://rfc-activity.azurewebsites.net/api/GetPendingApprovals");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    return json;
                }

                return $"Received status code: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
