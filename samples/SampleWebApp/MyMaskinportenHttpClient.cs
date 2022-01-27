using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleWebApp
{
    public class MyMaskinportenHttpClient
    {
        private readonly HttpClient _httpClient;

        public MyMaskinportenHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> PerformStuff(string url)
        {
            return await _httpClient.GetAsync(url);
        }
    }
}
