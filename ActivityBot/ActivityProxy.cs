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
            _httpClient.DefaultRequestHeaders.Add("Secret", Environment.GetEnvironmentVariable("MSI_SECRET"));
        }

        public async Task<string> GetAwaiting()
        {
            try
            {
                string resource = "https://rfc-activity.azurewebsites.net/api/GetPendingApprovals";
                string apiversion = "2017-09-01";

                var response = await _httpClient.GetAsync(String.Format("{0}/?resource={1}&api-version={2}", Environment.GetEnvironmentVariable("MSI_ENDPOINT"), resource, apiversion));
                //var response = await _httpClient.GetAsync("https://rfc-activity.azurewebsites.net/api/GetPendingApprovals");

                //if (response.IsSuccessStatusCode)
                //{
                    var content = await response.Content.ReadAsStringAsync();
                //
                //    return json;
                //}

                return $"Received status code: {response.StatusCode} - {content}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
