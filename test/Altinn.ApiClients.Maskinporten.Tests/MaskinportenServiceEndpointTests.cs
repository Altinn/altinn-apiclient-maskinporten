using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Altinn.ApiClients.Maskinporten.Tests
{
    public class MaskinportenServiceEndpointTests
    {
        private const string MaskinportenTokenJson =
            "{\"access_token\":\"maskinporten-token\",\"token_type\":\"Bearer\",\"expires_in\":120,\"scope\":\"altinn:test\"}";

        private static readonly string EncodedJwk = CreateEncodedJwk();

        [Theory]
        [InlineData("prod", "https://maskinporten.no/")]
        [InlineData("ver1", "https://ver1.maskinporten.no/")]
        [InlineData("ver2", "https://ver2.maskinporten.no/")]
        [InlineData("test", "https://test.maskinporten.no/")]
        public async Task GetToken_NoOverrides_UsesEndpointAndAudienceDerivedFromEnvironment(string environment, string expectedIssuer)
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings(environment);

            TokenResponse token = await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal("maskinporten-token", token.AccessToken);
            HttpRequestMessage request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(new Uri(expectedIssuer + "token"), request.RequestUri);
            Assert.Equal(expectedIssuer, GetAssertionAudience(handler.RequestBodies[0]));
        }

        [Fact]
        public async Task GetToken_UnknownEnvironmentWithoutOverrides_ThrowsArgumentException()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings("local");

            await Assert.ThrowsAsync<ArgumentException>(() => CreateService(handler).GetToken(CreateClientDefinition(settings)));
            Assert.Empty(handler.Requests);
        }

        [Fact]
        public async Task GetToken_TokenEndpointAndAudienceOverridden_EnvironmentIsNotRequired()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings(null);
            settings.TokenEndpoint = "http://localhost:5097/token";
            settings.AssertionAudience = "http://localhost:5097/";

            TokenResponse token = await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal("maskinporten-token", token.AccessToken);
            Assert.Equal(120, token.ExpiresIn);
            HttpRequestMessage request = Assert.Single(handler.Requests);
            Assert.Equal(new Uri("http://localhost:5097/token"), request.RequestUri);

            Dictionary<string, string> form = ParseForm(handler.RequestBodies[0]);
            Assert.Equal("urn:ietf:params:oauth:grant-type:jwt-bearer", form["grant_type"]);
            JwtSecurityToken assertion = new JwtSecurityTokenHandler().ReadJwtToken(form["assertion"]);
            Assert.Equal("http://localhost:5097/", assertion.Payload["aud"]);
            Assert.Equal("test-client", assertion.Payload["iss"]);
            Assert.Equal("altinn:test", assertion.Payload["scope"]);
        }

        [Fact]
        public async Task GetToken_OnlyTokenEndpointOverridden_AudienceIsDerivedFromEnvironment()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings("ver2");
            settings.TokenEndpoint = "http://localhost:5097/token";

            await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal(new Uri("http://localhost:5097/token"), Assert.Single(handler.Requests).RequestUri);
            Assert.Equal("https://ver2.maskinporten.no/", GetAssertionAudience(handler.RequestBodies[0]));
        }

        [Fact]
        public async Task GetToken_OnlyAudienceOverriddenWithUnknownEnvironment_ThrowsArgumentException()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings("local");
            settings.AssertionAudience = "http://localhost:5097/";

            await Assert.ThrowsAsync<ArgumentException>(() => CreateService(handler).GetToken(CreateClientDefinition(settings)));
        }

        [Fact]
        public async Task GetToken_TokenExchangeWithoutOverride_UsesEndpointDerivedFromEnvironment()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson, "\"altinn-token\"");
            var settings = CreateSettings("test");
            settings.ExhangeToAltinnToken = true;
            settings.TokenExchangeEnvironment = "at22";

            TokenResponse token = await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal("altinn-token", token.AccessToken);
            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal(
                new Uri("https://platform.at22.altinn.cloud/authentication/api/v1/exchange/maskinporten"),
                handler.Requests[1].RequestUri);
        }

        [Fact]
        public async Task GetToken_TokenExchangeEndpointOverridden_ExchangesAgainstConfiguredEndpoint()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson, "\"altinn-token\"");
            var settings = CreateSettings(null);
            settings.TokenEndpoint = "http://localhost:5097/token";
            settings.AssertionAudience = "http://localhost:5097/";
            settings.ExhangeToAltinnToken = true;
            settings.UseAltinnTestOrg = true;
            settings.TokenExchangeEndpoint = "http://localhost:5098/authentication/api/v1/exchange/maskinporten";

            TokenResponse token = await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal("altinn-token", token.AccessToken);
            Assert.Equal("altinn", token.TokenType);
            Assert.Equal(2, handler.Requests.Count);
            HttpRequestMessage exchangeRequest = handler.Requests[1];
            Assert.Equal(HttpMethod.Get, exchangeRequest.Method);
            Assert.Equal(
                new Uri("http://localhost:5098/authentication/api/v1/exchange/maskinporten?test=true"),
                exchangeRequest.RequestUri);
            Assert.Equal("Bearer", exchangeRequest.Headers.Authorization.Scheme);
            Assert.Equal("maskinporten-token", exchangeRequest.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public async Task GetToken_BlankOverrides_FallBackToEnvironment(string blank)
        {
            var handler = new RecordingHandler(MaskinportenTokenJson, "\"altinn-token\"");
            var settings = CreateSettings("test");
            settings.TokenEndpoint = blank;
            settings.AssertionAudience = blank;
            settings.ExhangeToAltinnToken = true;
            settings.TokenExchangeEndpoint = blank;

            await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal(new Uri("https://test.maskinporten.no/token"), handler.Requests[0].RequestUri);
            Assert.Equal("https://test.maskinporten.no/", GetAssertionAudience(handler.RequestBodies[0]));
            Assert.Equal(
                new Uri("https://platform.tt02.altinn.no/authentication/api/v1/exchange/maskinporten"),
                handler.Requests[1].RequestUri);
        }

        [Theory]
        [InlineData("/token")]
        [InlineData("localhost:5097/token")]
        [InlineData("ftp://localhost:5097/token")]
        [InlineData("not a url")]
        public async Task GetToken_InvalidTokenEndpoint_ThrowsArgumentExceptionNamingTheSetting(string tokenEndpoint)
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings("test");
            settings.TokenEndpoint = tokenEndpoint;

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => CreateService(handler).GetToken(CreateClientDefinition(settings)));

            Assert.Contains("TokenEndpoint", exception.Message);
            Assert.Empty(handler.Requests);
        }

        [Fact]
        public async Task GetToken_InvalidTokenExchangeEndpoint_ThrowsArgumentExceptionNamingTheSetting()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings("test");
            settings.ExhangeToAltinnToken = true;
            settings.TokenExchangeEndpoint = "/authentication/api/v1/exchange/maskinporten";

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => CreateService(handler).GetToken(CreateClientDefinition(settings)));

            Assert.Contains("TokenExchangeEndpoint", exception.Message);
            Assert.Single(handler.Requests);
        }

        [Fact]
        public async Task GetToken_OnlyTokenEndpointOverriddenWithoutEnvironment_ErrorMentionsAssertionAudience()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            var settings = CreateSettings(null);
            settings.TokenEndpoint = "http://localhost:5097/token";

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => CreateService(handler).GetToken(CreateClientDefinition(settings)));

            Assert.Contains("AssertionAudience", exception.Message);
        }

        [Fact]
        public async Task GetToken_SameClientWithDifferentTokenEndpoints_DoesNotShareCachedToken()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson, MaskinportenTokenJson);
            MaskinportenService service = CreateService(handler);
            var realSettings = CreateSettings("test");
            var mockSettings = CreateSettings("test");
            mockSettings.TokenEndpoint = "http://localhost:5097/token";

            await service.GetToken(CreateClientDefinition(realSettings));
            await service.GetToken(CreateClientDefinition(mockSettings));
            await service.GetToken(CreateClientDefinition(realSettings));
            await service.GetToken(CreateClientDefinition(mockSettings));

            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal(new Uri("https://test.maskinporten.no/token"), handler.Requests[0].RequestUri);
            Assert.Equal(new Uri("http://localhost:5097/token"), handler.Requests[1].RequestUri);
        }

        [Fact]
        public async Task GetToken_SameTokenWithDifferentTokenExchangeEndpoints_DoesNotShareCachedExchange()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson, "\"altinn-token-1\"", "\"altinn-token-2\"");
            MaskinportenService service = CreateService(handler);
            var firstSettings = CreateSettings("test");
            firstSettings.ExhangeToAltinnToken = true;
            firstSettings.TokenExchangeEndpoint = "http://localhost:5098/exchange";
            var secondSettings = CreateSettings("test");
            secondSettings.ExhangeToAltinnToken = true;
            secondSettings.TokenExchangeEndpoint = "http://localhost:5099/exchange";

            TokenResponse first = await service.GetToken(CreateClientDefinition(firstSettings));
            TokenResponse second = await service.GetToken(CreateClientDefinition(secondSettings));

            Assert.Equal("altinn-token-1", first.AccessToken);
            Assert.Equal("altinn-token-2", second.AccessToken);
            Assert.Equal(3, handler.Requests.Count);
            Assert.Equal(new Uri("http://localhost:5099/exchange"), handler.Requests[2].RequestUri);
        }

        [Fact]
        public async Task GetToken_TestOrgWithTokenExchangeEndpointContainingQuery_AppendsTestFlagToQuery()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson, "\"altinn-token\"");
            var settings = CreateSettings("test");
            settings.ExhangeToAltinnToken = true;
            settings.UseAltinnTestOrg = true;
            settings.TokenExchangeEndpoint = "http://localhost:5098/exchange?tenant=local";

            await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal(new Uri("http://localhost:5098/exchange?tenant=local&test=true"), handler.Requests[1].RequestUri);
        }

        [Fact]
        public async Task GetToken_TestOrgWithoutOverride_AppendsTestFlag()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson, "\"altinn-token\"");
            var settings = CreateSettings("test");
            settings.ExhangeToAltinnToken = true;
            settings.UseAltinnTestOrg = true;

            await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Equal(
                new Uri("https://platform.tt02.altinn.no/authentication/api/v1/exchange/maskinporten?test=true"),
                handler.Requests[1].RequestUri);
        }

        [Fact]
        public void MaskinportenSettings_EndpointOverrides_CanBeBoundFromConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["MaskinportenSettings:TokenEndpoint"] = "http://localhost:5097/token",
                    ["MaskinportenSettings:AssertionAudience"] = "http://localhost:5097/",
                    ["MaskinportenSettings:TokenExchangeEndpoint"] = "http://localhost:5098/exchange",
                })
                .Build();

            var settings = new MaskinportenSettings();
            configuration.GetSection("MaskinportenSettings").Bind(settings);

            Assert.Equal("http://localhost:5097/token", settings.TokenEndpoint);
            Assert.Equal("http://localhost:5097/", settings.AssertionAudience);
            Assert.Equal("http://localhost:5098/exchange", settings.TokenExchangeEndpoint);
        }

        [Fact]
        public async Task GetToken_CustomSettingsWithoutEndpointProperties_KeepsDefaultBehaviour()
        {
            var handler = new RecordingHandler(MaskinportenTokenJson);
            IMaskinportenSettings settings = new LegacySettings
            {
                ClientId = "test-client",
                Scope = "altinn:test",
                Environment = "test",
                EncodedJwk = EncodedJwk,
            };

            await CreateService(handler).GetToken(CreateClientDefinition(settings));

            Assert.Null(settings.TokenEndpoint);
            Assert.Equal(new Uri("https://test.maskinporten.no/token"), Assert.Single(handler.Requests).RequestUri);
        }

        private static MaskinportenService CreateService(RecordingHandler handler)
        {
            return new MaskinportenService(
                new HttpClient(handler),
                NullLogger<MaskinportenService>.Instance,
                new MemoryTokenCacheProvider(new MemoryCache(new MemoryCacheOptions())));
        }

        private static MaskinportenSettings CreateSettings(string environment)
        {
            return new MaskinportenSettings
            {
                ClientId = "test-client",
                Scope = "altinn:test",
                Environment = environment,
                EncodedJwk = EncodedJwk,
            };
        }

        private static SettingsJwkClientDefinition CreateClientDefinition(IMaskinportenSettings settings)
        {
            return new SettingsJwkClientDefinition { ClientSettings = settings };
        }

        private static string GetAssertionAudience(string requestBody)
        {
            return (string)new JwtSecurityTokenHandler().ReadJwtToken(ParseForm(requestBody)["assertion"]).Payload["aud"];
        }

        private static Dictionary<string, string> ParseForm(string body)
        {
            return body.Split('&')
                .Select(pair => pair.Split('='))
                .ToDictionary(kv => WebUtility.UrlDecode(kv[0]), kv => WebUtility.UrlDecode(kv[1]));
        }

        private static string CreateEncodedJwk()
        {
            using RSA rsa = RSA.Create(2048);
            RSAParameters p = rsa.ExportParameters(true);
            string json = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["kty"] = "RSA",
                ["kid"] = "test-kid",
                ["n"] = Base64UrlEncoder.Encode(p.Modulus),
                ["e"] = Base64UrlEncoder.Encode(p.Exponent),
                ["d"] = Base64UrlEncoder.Encode(p.D),
                ["p"] = Base64UrlEncoder.Encode(p.P),
                ["q"] = Base64UrlEncoder.Encode(p.Q),
                ["dp"] = Base64UrlEncoder.Encode(p.DP),
                ["dq"] = Base64UrlEncoder.Encode(p.DQ),
                ["qi"] = Base64UrlEncoder.Encode(p.InverseQ),
            });
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// Returns the given response bodies in order and records each request.
        /// </summary>
        private class RecordingHandler : HttpMessageHandler
        {
            private readonly Queue<string> _responses;

            public RecordingHandler(params string[] responses)
            {
                _responses = new Queue<string>(responses);
            }

            public List<HttpRequestMessage> Requests { get; } = new();

            public List<string> RequestBodies { get; } = new();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                RequestBodies.Add(request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken));
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_responses.Dequeue(), Encoding.UTF8, "application/json")
                };
            }
        }

        /// <summary>
        /// A settings implementation written against the interface before the endpoint overrides were added.
        /// </summary>
        private class LegacySettings : IMaskinportenSettings
        {
            public string ClientId { get; set; }
            public string Scope { get; set; }
            public string Resource { get; set; }
            public string Environment { get; set; }
            public string CertificatePkcs12Path { get; set; }
            public string CertificatePkcs12Password { get; set; }
            public string CertificateStoreThumbprint { get; set; }
            public string EncodedJwk { get; set; }
            public string EncodedX509 { get; set; }
            public string Kid { get; set; }
            public string ConsumerOrgNo { get; set; }
            public string EnterpriseUserName { get; set; }
            public string EnterpriseUserPassword { get; set; }
            public bool? ExhangeToAltinnToken { get; set; }
            public string TokenExchangeEnvironment { get; set; }
            public bool? UseAltinnTestOrg { get; set; }
            public bool? EnableDebugLogging { get; set; }
            public bool? OverwriteAuthorizationHeader { get; set; }
        }
    }
}
