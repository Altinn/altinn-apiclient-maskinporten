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


    public class MyOtherMaskinportenHttpClient : MyMaskinportenHttpClient
    {
        public MyOtherMaskinportenHttpClient(HttpClient httpClient) : base(httpClient) { }
    }

    public class MyThirdMaskinportenHttpClient : MyMaskinportenHttpClient
    {
        public MyThirdMaskinportenHttpClient(HttpClient httpClient) : base(httpClient) { }
    }

    public class MyFourthMaskinportenHttpClient : MyMaskinportenHttpClient
    {
        public MyFourthMaskinportenHttpClient(HttpClient httpClient) : base(httpClient) { }
    }
}
