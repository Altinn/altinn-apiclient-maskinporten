using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MaskinportenTestController : ControllerBase
    {
        private readonly MyMaskinportenHttpClient _myMaskinportenHttpClient;
        private readonly MySystemUserMaskinportenHttpClient _mySystemUserMaskinportenHttpClient;
        // private readonly IMyOtherMaskinportenHttpClient _myOtherMaskinportenHttpClient;
        // private readonly MyThirdMaskinportenHttpClient _myThirdMaskinportenHttpClient;
        // private readonly MyFourthMaskinportenHttpClient _myFourthMaskinportenHttpClient;

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public MaskinportenTestController(
            MyMaskinportenHttpClient myMaskinportenHttpClient,
            MySystemUserMaskinportenHttpClient mySystemUserMaskinportenHttpClient,
            // IMyOtherMaskinportenHttpClient myOtherMaskinportenHttpClient,
            // MyThirdMaskinportenHttpClient myThirdMaskinportenHttpClient,
            // MyFourthMaskinportenHttpClient myFourthMaskinportenHttpClient,
            IConfiguration configuration,
            IHttpClientFactory clientFactory)
        {
            // These are the injected typed clients created in Program.cs
            _myMaskinportenHttpClient = myMaskinportenHttpClient;
            _mySystemUserMaskinportenHttpClient = mySystemUserMaskinportenHttpClient;
            // _myOtherMaskinportenHttpClient = myOtherMaskinportenHttpClient;
            // _myThirdMaskinportenHttpClient = myThirdMaskinportenHttpClient;
            // _myFourthMaskinportenHttpClient = myFourthMaskinportenHttpClient;

            _configuration = configuration;
            // Get the factory as well so we can get our named clients
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<object>> Get()
        {
            var targetUrl = GetTargetUrl();
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                return BadRequest("Missing configuration value 'SampleWebApp:TargetUrl'.");
            }
/*
            // Here we instantiate a named client as defined in Program.cs
            var client0 = _clientFactory.CreateClient("myhttpclient1");
            var result0 = await client0.GetAsync(targetUrl + "?myhttpclient");

            var client1 = _clientFactory.CreateClient("myhttpclient2");
            var result1 = await client1.GetAsync(targetUrl + "?myhttpclient2");

            // Perform some requests with both the named client and the type client. This will both use the same token. 

            var result2 = await _myMaskinportenHttpClient.PerformStuff(targetUrl + "?_myMaskinportenHttpClient");
            var result3 = await _myOtherMaskinportenHttpClient.PerformStuff(targetUrl + "?_myOtherMaskinportenHttpClient");
            var result4 = await _myThirdMaskinportenHttpClient.PerformStuff(targetUrl + "?_myThirdMaskinportenHttpClient");
            var result5 = await _myFourthMaskinportenHttpClient.PerformStuff(targetUrl + "?_myFourthMaskinportenHttpClient");
*/

            var response = await _myMaskinportenHttpClient.PerformStuff(targetUrl + "?_myMaskinportenHttpClient");
            var responseBody = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, new
            {
                Example = "maskinporten-basic",
                TargetUrl = targetUrl,
                DownstreamStatusCode = (int)response.StatusCode,
                DownstreamReasonPhrase = response.ReasonPhrase,
                DownstreamBody = responseBody
            });
        }

        [HttpGet("systemuser")]
        public async Task<ActionResult<object>> GetSystemUserExample([FromQuery] string customerOrgNo, [FromQuery] string externalRef = null)
        {
            if (string.IsNullOrWhiteSpace(customerOrgNo))
            {
                return BadRequest("Missing required query parameter 'customerOrgNo'.");
            }

            var targetUrl = GetTargetUrl();
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                return BadRequest("Missing configuration value 'SampleWebApp:TargetUrl'.");
            }

            var response = await _mySystemUserMaskinportenHttpClient.PerformStuff(targetUrl, customerOrgNo, externalRef);
            var responseBody = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, new
            {
                Example = "systemuser-altinn-exchange",
                CustomerOrgNo = customerOrgNo,
                ExternalRef = externalRef,
                TargetUrl = targetUrl,
                DownstreamStatusCode = (int)response.StatusCode,
                DownstreamReasonPhrase = response.ReasonPhrase,
                DownstreamBody = responseBody
            });
        }

        private string GetTargetUrl()
        {
            return _configuration["SampleWebApp:TargetUrl"];
        }
    }
}
