﻿using Microsoft.AspNetCore.Mvc;
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
            var clientFoo = _clientFactory.CreateClient("foo");
            var clientBar = _clientFactory.CreateClient("bar");

            var resultFoo = await clientFoo.GetAsync("https://ent878u4vial.x.pipedream.net");
            var resultBar = await clientBar.GetAsync("https://ent878u4vial.x.pipedream.net");

            return "Done";

        }
    }
}
