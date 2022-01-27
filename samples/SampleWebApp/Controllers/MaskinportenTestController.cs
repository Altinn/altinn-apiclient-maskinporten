using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MaskinportenTestController : ControllerBase
    {
        private readonly ILogger<MaskinportenTestController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly MyMaskinportenHttpClient _myMaskinportenHttpClient;

        public MaskinportenTestController(ILogger<MaskinportenTestController> logger, IHttpClientFactory clientFactory, MyMaskinportenHttpClient myMaskinportenHttpClient)
        {
            _logger = logger;
            _clientFactory = clientFactory;

            // This is the injected typed client created in Startup.cs
            _myMaskinportenHttpClient = myMaskinportenHttpClient;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            // You can use something like https://requestbin.com to see what headers are sent
            var url = "https://someurlfromrequestbin";

            // Here we instantiate a named client as defined in Startup.cs
            var client1 = _clientFactory.CreateClient("myhttpclient");
            
            // Perform some requests with both the named client and the type client. This will both use the same token. 
            var result1 = await client1.GetAsync(url);
            var result2 = await _myMaskinportenHttpClient.PerformStuff(url);

            return "Done!";

        }
    }
}
