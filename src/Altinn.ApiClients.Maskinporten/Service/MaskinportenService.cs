using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Models;
using Altinn.ApiClients.Maskinporten.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public class MaskinportenService: IMaskinporten
    {
        private readonly HttpClient _client;

        private readonly MaskinportenSettings _maskinportenConfig;

        private readonly ILogger _logger;

        private readonly IClientSecret _clientSecret;

        private readonly IMemoryCache _memoryCache;

        public MaskinportenService(HttpClient httpClient, 
            IOptions<MaskinportenSettings> maskinportenConfig, 
            ILogger<MaskinportenService> logger,
            IMemoryCache memoryCache,
            IClientSecret clientSecret)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
            _maskinportenConfig = maskinportenConfig.Value;
            _logger = logger;
            _memoryCache = memoryCache;
            _clientSecret = clientSecret;
        }

     
        public async Task<TokenResponse> GetToken(X509Certificate2 cert, string clientId, string scope, string resource, bool disableCaching = false)
        {
            return await GetToken(cert, null, clientId, scope, resource, disableCaching);
        }

        public async Task<TokenResponse> GetToken(JsonWebKey jwk, string clientId, string scope, string resource, bool disableCaching = false)
        {
            return await GetToken(null, jwk, clientId, scope, resource, disableCaching);
        }

        public async Task<TokenResponse> GetToken(string base64EncodedJwk, string clientId, string scope, string resource, bool disableCaching = false)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedJwk);
            string jwkjson = Encoding.UTF8.GetString(base64EncodedBytes);
            JsonWebKey jwk = new JsonWebKey(jwkjson);
            return await GetToken(null, jwk, clientId, scope, resource, disableCaching);
        }

        public async Task<TokenResponse> GetToken(bool disableCaching = false)
        {
            ClientSecrets clientSecrets = await _clientSecret.GetClientSecrets();
            if (clientSecrets.ClientKey != null)
            {
                return await GetToken(null, clientSecrets.ClientKey, _maskinportenConfig.ClientId, _maskinportenConfig.Scope, _maskinportenConfig.Resource, disableCaching);
            }
            else
            {
                return await GetToken(clientSecrets.ClientCertificate, null , _maskinportenConfig.ClientId, _maskinportenConfig.Scope, _maskinportenConfig.Resource, disableCaching);
            }
        }

        private async Task<TokenResponse> GetToken(X509Certificate2 cert, JsonWebKey jwk, string clientId, string scope, string resource, bool disableCaching)
        {
            string cacheKey = $"{clientId}-{scope}";
            TokenResponse accesstokenResponse;
            if (disableCaching || !_memoryCache.TryGetValue(cacheKey, out accesstokenResponse))
            {
                string jwtAssertion = GetJwtAssertion(cert, jwk, clientId, scope, resource);
                FormUrlEncodedContent content = GetUrlEncodedContent(jwtAssertion);
                accesstokenResponse = await PostToken(content);

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                {
                    Priority = CacheItemPriority.High,
                };
                cacheEntryOptions.SetAbsoluteExpiration(new TimeSpan(0, 0, accesstokenResponse.ExpiresIn - 30));
                _memoryCache.Set(cacheKey, accesstokenResponse, cacheEntryOptions);
            }

            return accesstokenResponse;
        }

        public string GetJwtAssertion(X509Certificate2 cert, JsonWebKey jwk, string clientId, string scope, string resource)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);
            JwtHeader header;
            if (cert != null)
            {
                header = GetHeader(cert);
            }
            else
            {
                header = GetHeader(jwk);
            }

            JwtPayload payload = new JwtPayload
            {
                { "aud", GetAssertionAud() },
                { "scope", scope },
                { "iss", clientId },
                { "exp", dateTimeOffset.ToUnixTimeSeconds() + 10 },
                { "iat", dateTimeOffset.ToUnixTimeSeconds() },
                { "jti", Guid.NewGuid().ToString() },
            };
            
            if (string.IsNullOrEmpty(resource))
            {
                payload.Add("resource", resource);
            }

            JwtSecurityToken securityToken = new JwtSecurityToken(header, payload);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(securityToken);
        }

        private JwtHeader GetHeader(JsonWebKey jwk)
        {
            JwtHeader header = new JwtHeader(new SigningCredentials(jwk, SecurityAlgorithms.RsaSha256));
            return header;
        }

        private JwtHeader GetHeader(X509Certificate2 cert)
        {
            X509SecurityKey securityKey = new X509SecurityKey(cert);
            JwtHeader header = new JwtHeader(new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256))
            {
                { "x5c", new List<string>() { Convert.ToBase64String(cert.GetRawCertData()) } }
            };
            header.Remove("typ");
            header.Remove("kid");
            return header;
        }

        private FormUrlEncodedContent GetUrlEncodedContent(string assertion)
        {
            FormUrlEncodedContent formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("assertion", assertion),
            });

            return formContent;
        }

        public async Task<TokenResponse> PostToken(FormUrlEncodedContent bearer)
        {
            TokenResponse token = null; ;

            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(GetTokenEndpoint()),
                Content = bearer
            };

            HttpResponseMessage response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                token = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(content);
                return token;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogError(response.StatusCode + " " + error);
            }

            return null;
        }

        private string GetAssertionAud()
        {
            if (_maskinportenConfig.Environment.Equals("prod"))
            {
                return _maskinportenConfig.JwtAssertionAudienceProd;
            }
            else if (_maskinportenConfig.Environment.Equals("ver1"))
            {
                return _maskinportenConfig.JwtAssertionAudienceVer1;
            }
            else if (_maskinportenConfig.Environment.Equals("ver2"))
            {
                return _maskinportenConfig.JwtAssertionAudienceVer2;
            }

            return null;
        }

        private string GetTokenEndpoint()
        {
           if(_maskinportenConfig.Environment.Equals("prod"))
            {
                return _maskinportenConfig.TokenEndpointProd;
            }
            else if (_maskinportenConfig.Environment.Equals("ver1"))
            {
                return _maskinportenConfig.TokenEndpointVer1;
            }
            else if (_maskinportenConfig.Environment.Equals("ver2"))
            {
                return _maskinportenConfig.TokenEndpointVer2;
            }

           return null;
        }

    }
}
