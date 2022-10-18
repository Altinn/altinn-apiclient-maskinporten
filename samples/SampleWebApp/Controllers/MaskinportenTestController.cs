using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MaskinportenTestController : ControllerBase
    {
        private readonly MyMaskinportenHttpClient _myMaskinportenHttpClient;
        private readonly MyOtherMaskinportenHttpClient _myOtherMaskinportenHttpClient;
        private readonly MyThirdMaskinportenHttpClient _myThirdMaskinportenHttpClient;
        private readonly MyFourthMaskinportenHttpClient _myFourthMaskinportenHttpClient;

        private readonly IHttpClientFactory _clientFactory;

        public MaskinportenTestController(
            MyMaskinportenHttpClient myMaskinportenHttpClient,
            MyOtherMaskinportenHttpClient myOtherMaskinportenHttpClient,
            MyThirdMaskinportenHttpClient myThirdMaskinportenHttpClient,
            MyFourthMaskinportenHttpClient myFourthMaskinportenHttpClient,
            IHttpClientFactory clientFactory)
        {
            // These are the injected typed client created in Startup.cs
            _myMaskinportenHttpClient = myMaskinportenHttpClient;
            _myOtherMaskinportenHttpClient = myOtherMaskinportenHttpClient;
            _myThirdMaskinportenHttpClient = myThirdMaskinportenHttpClient;
            _myFourthMaskinportenHttpClient = myFourthMaskinportenHttpClient;

            // Get the factory as well so we can get our named clients
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            // You can use something like https://requestbin.com to see what headers are sent
            var url = "https://eoeauwtxq2wgimb.m.pipedream.net";

            // Here we instantiate a named client as defined in Startup.cs
            var client0 = _clientFactory.CreateClient("myhttpclient");
            var result0 = await client0.GetAsync(url + "?myhttpclient");

            var client1 = _clientFactory.CreateClient("myotherhttpclient");
            var result1 = await client1.GetAsync(url + "?myotherhttpclient");

            // Perform some requests with both the named client and the type client. This will both use the same token. 

            var result2 = await _myMaskinportenHttpClient.PerformStuff(url + "?_myMaskinportenHttpClient");
            var result3 = await _myOtherMaskinportenHttpClient.PerformStuff(url + "?_myOtherMaskinportenHttpClient");
            var result4 = await _myThirdMaskinportenHttpClient.PerformStuff(url + "?_myThirdMaskinportenHttpClient");
            var result5 = await _myFourthMaskinportenHttpClient.PerformStuff(url + "?_myFourthMaskinportenHttpClient");

            return "Done!";
        }
    }
}
