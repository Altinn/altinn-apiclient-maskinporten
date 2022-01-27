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
            // See https://requestbin.com
            var url = "https://enk2l7xrl5ew.x.pipedream.net";

            // Here we instantiate a named client as defined in Startup.cs
           // var client1 = _clientFactory.CreateClient("myhttpclient");
            
            // Perform some requests with them both. 
           // var result1 = await client1.GetAsync(url);
            var result2 = await _myMaskinportenHttpClient.PerformStuff(url);

            return "Done, see https://requestbin.com/r/enk2l7xrl5ew";

        }
    }
}
