using ActivityBot.Activity.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ActivityBot.Activity.Proxy
{
    public class ActivityProxy : IActivityProxy
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public ActivityProxy(HttpClient httpClient, string url)
        {
            _httpClient = httpClient;
            _url = url;
        }

        public async Task<List<Awaiting>> GetAwaiting()
        {
            try
            {
                var response = await _httpClient.GetAsync(_url);

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<Awaiting>>(json);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
