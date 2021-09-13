using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Interfaces;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public class MaskinportenService: IMaskinportenService
    {
        private readonly HttpClient _client;

        private readonly ILogger _logger;

        private readonly IMemoryCache _memoryCache;

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public MaskinportenService(HttpClient httpClient, 
            ILogger<IMaskinportenService> logger,
            IMemoryCache memoryCache)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<TokenResponse> GetToken(X509Certificate2 cert, string environment, string clientId, string scope, string resource, bool disableCaching = false)
        {
            return await GetToken(cert, null, environment, clientId, scope, resource, disableCaching);
        }

        public async Task<TokenResponse> GetToken(JsonWebKey jwk, string environment, string clientId, string scope, string resource, bool disableCaching = false)
        {
            return await GetToken(null, jwk, environment, clientId, scope, resource, disableCaching);
        }

        public async Task<TokenResponse> GetToken(string base64EncodedJwk, string environment, string clientId, string scope, string resource, bool disableCaching = false)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedJwk);
            string jwkjson = Encoding.UTF8.GetString(base64EncodedBytes);
            JsonWebKey jwk = new JsonWebKey(jwkjson);
            return await GetToken(null, jwk, environment, clientId, scope, resource, disableCaching);
        }

        public async Task<TokenResponse> GetToken(IClientDefinition clientDefinition, bool disableCaching = false)
        {
            ClientSecrets clientSecrets = await clientDefinition.GetClientSecrets();
            if (clientSecrets.ClientKey != null)
            {
                return await GetToken(null, clientSecrets.ClientKey, clientDefinition.ClientSettings.Environment, clientDefinition.ClientSettings.ClientId, clientDefinition.ClientSettings.Scope, clientDefinition.ClientSettings.Resource, disableCaching);
            }

            return await GetToken(clientSecrets.ClientCertificate, null, clientDefinition.ClientSettings.Environment, clientDefinition.ClientSettings.ClientId, clientDefinition.ClientSettings.Scope, clientDefinition.ClientSettings.Resource, disableCaching);
        }

        private async Task<TokenResponse> GetToken(X509Certificate2 cert, JsonWebKey jwk, string environment, string clientId, string scope, string resource, bool disableCaching)
        {
            string cacheKey = $"{clientId}-{scope}-{resource}";
            TokenResponse accesstokenResponse;

            await SemaphoreSlim.WaitAsync();
            try
            {
                if (disableCaching || !_memoryCache.TryGetValue(cacheKey, out accesstokenResponse))
                {
                    string jwtAssertion = GetJwtAssertion(cert, jwk, environment, clientId, scope, resource);
                    FormUrlEncodedContent content = GetUrlEncodedContent(jwtAssertion);
                    accesstokenResponse = await PostToken(environment, content);

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    {
                        Priority = CacheItemPriority.High,
                    };
                    cacheEntryOptions.SetAbsoluteExpiration(new TimeSpan(0, 0, Math.Max(0, accesstokenResponse.ExpiresIn - 30)));
                    _memoryCache.Set(cacheKey, accesstokenResponse, cacheEntryOptions);
                }
            }
            finally
            {
                SemaphoreSlim.Release();
            }

            return accesstokenResponse;
        }

        public string GetJwtAssertion(X509Certificate2 cert, JsonWebKey jwk, string environment, string clientId, string scope, string resource)
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
                { "aud", GetAssertionAud(environment) },
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
            return new JwtHeader(new SigningCredentials(jwk, SecurityAlgorithms.RsaSha256));
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

        public async Task<TokenResponse> PostToken(string environment, FormUrlEncodedContent bearer)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(GetTokenEndpoint(environment)),
                Content = bearer
            };

            HttpResponseMessage response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                string successResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TokenResponse>(successResponse);
            }
            
            string errorResponse = await response.Content.ReadAsStringAsync();
            ErrorReponse error = JsonSerializer.Deserialize<ErrorReponse>(errorResponse) ?? new ErrorReponse()
            {
                ErrorType = "deserializing",
                Description = "Unable to deserialize error from Maskinporten. Received: " +
                              (string.IsNullOrEmpty(errorResponse) ? "<empty>" : errorResponse)
            };

            _logger.LogError("errorType={errorType} description={description} statuscode={statusCode}", error.ErrorType, error.Description, response.StatusCode);
            throw new TokenRequestException(error.Description);
        }

        private string GetAssertionAud(string environment)
        {
            return environment switch
            {
                "prod" => "https://maskinporten.no/",
                "ver1" => "https://ver1.maskinporten.no/",
                "ver2" => "https://ver2.maskinporten.no/",
                _ => throw new ArgumentException("Invalid environment setting. Valid values: prod, ver1, ver2")
            };
        }

        private string GetTokenEndpoint(string environment)
        {
            return environment switch
            {
                "prod" => "https://maskinporten.no/token",
                "ver1" => "https://ver1.maskinporten.no/token",
                "ver2" => "https://ver2.maskinporten.no/token",
                _ => throw new ArgumentException("Invalid environment setting. Valid values: prod, ver1, ver2")
            };
        }
    }
}
