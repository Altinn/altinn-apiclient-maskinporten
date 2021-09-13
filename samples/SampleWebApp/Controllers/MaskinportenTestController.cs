using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Services;

namespace SampleWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MaskinportenTestController : ControllerBase
    {
        private readonly ILogger<MaskinportenTestController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public MaskinportenTestController(ILogger<MaskinportenTestController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<string> Get()
        {
           var client1 = _clientFactory.CreateClient("client1");
           var client2 = _clientFactory.CreateClient("client2");
           var client3 = _clientFactory.CreateClient("client3");
           var client4 = _clientFactory.CreateClient("client4");
           var client5 = _clientFactory.CreateClient("client5");

           // var result1 = await client1.GetAsync("https://ent878u4vial.x.pipedream.net");
           var result2 = await client2.GetAsync("https://en07biquml4v5n.x.pipedream.net");

            return "Done";

        }
    }
}
