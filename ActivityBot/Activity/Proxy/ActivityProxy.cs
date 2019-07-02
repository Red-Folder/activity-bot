using ActivityBot.Activity.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ActivityBot.Activity.Proxy
{
    public class ActivityProxy : IActivityProxy
    {
        public const string HTTP_CLIENT_NAME = "ActivityProxy";

        private readonly HttpClient _httpClient;
        private readonly ActivityProxyConfiguration _configuration;

        public ActivityProxy(IHttpClientFactory clientFactory, ActivityProxyConfiguration configuration)
        {
            _httpClient = clientFactory.CreateClient(HTTP_CLIENT_NAME);
            _configuration = configuration;
        }

        public async Task Approve(ApproveRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_configuration.ApproveUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Awaiting>> GetAwaiting()
        {
            try
            {
                var response = await _httpClient.GetAsync(_configuration.GetPendingApprovalsUrl);

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<Awaiting>>(json);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ManuallyTriggerWeeklyActivity(ManuallyTriggerWeeklyActivityRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_configuration.ManuallyTriggerWeeklyActivityUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
