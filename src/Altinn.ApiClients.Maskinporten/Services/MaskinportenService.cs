using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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

        private readonly ITokenCacheProvider _tokenCacheProvider;

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public MaskinportenService(HttpClient httpClient, 
            ILogger<IMaskinportenService> logger,
            ITokenCacheProvider tokenCacheProvider)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
            _logger = logger;
            _tokenCacheProvider = tokenCacheProvider;
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
            TokenResponse tokenResponse;
            if (clientSecrets.ClientKey != null)
            {
                tokenResponse = await GetToken(null, clientSecrets.ClientKey, clientDefinition.ClientSettings.Environment, clientDefinition.ClientSettings.ClientId, clientDefinition.ClientSettings.Scope, clientDefinition.ClientSettings.Resource, disableCaching);
            }
            else
            {
                tokenResponse = await GetToken(clientSecrets.ClientCertificate, null,
                    clientDefinition.ClientSettings.Environment, clientDefinition.ClientSettings.ClientId,
                    clientDefinition.ClientSettings.Scope, clientDefinition.ClientSettings.Resource, disableCaching);
            }

            if (!string.IsNullOrEmpty(clientDefinition.ClientSettings.EnterpriseUserName) &&
                !string.IsNullOrEmpty(clientDefinition.ClientSettings.EnterpriseUserPassword))
            {
                return await ExchangeToAltinnToken(tokenResponse, clientDefinition.ClientSettings.Environment, clientDefinition.ClientSettings.EnterpriseUserName,
                    clientDefinition.ClientSettings.EnterpriseUserPassword, disableCaching);
            }
            
            if (clientDefinition.ClientSettings.ExhangeToAltinnToken.HasValue &&
                     clientDefinition.ClientSettings.ExhangeToAltinnToken.Value)
            {
                return await ExchangeToAltinnToken(tokenResponse, clientDefinition.ClientSettings.Environment, disableCaching: disableCaching);
            }

            return tokenResponse;
        }

        public async Task<TokenResponse> ExchangeToAltinnToken(TokenResponse tokenResponse,
            string environment, string userName = null, string password = null, bool disableCaching = false)
        {
            string cacheKey = GetCacheKeyForTokenAndUsername(tokenResponse, userName ?? string.Empty);
            await SemaphoreSlim.WaitAsync();
            try
            {
                if (!disableCaching)
                {
                    (bool hasCachedValue, TokenResponse cachedTokenResponse) = await _tokenCacheProvider.TryGetToken(cacheKey);
                    if (hasCachedValue)
                    {
                        return cachedTokenResponse;
                    }
                }

                HttpRequestMessage requestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(GetTokenExchangeEndpoint(environment)),
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken) }
                };

                TokenResponse exchangedTokenResponse = new TokenResponse
                {
                    ExpiresIn = tokenResponse.ExpiresIn,
                    Scope = tokenResponse.Scope,
                    TokenType = "altinn"
                };

                if (userName != null && password != null)
                {
                    requestMessage.Headers.TryAddWithoutValidation("X-Altinn-EnterpriseUser-Authentication",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));
                }

                exchangedTokenResponse.AccessToken = await PerformRequest<string>(requestMessage);
                await _tokenCacheProvider.Set(cacheKey, exchangedTokenResponse,
                   new TimeSpan(0, 0, Math.Max(0, exchangedTokenResponse.ExpiresIn - 5)));

                return exchangedTokenResponse;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        private string GetCacheKeyForTokenAndUsername(TokenResponse tokenResponse, string userName)
        {
            MD5 md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(tokenResponse.AccessToken + userName)));
        }

        private string GetJwtAssertion(X509Certificate2 cert, JsonWebKey jwk, string environment, string clientId, string scope, string resource)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);
            JwtHeader header = cert != null ? GetHeader(cert) : GetHeader(jwk);

            JwtPayload payload = new JwtPayload
            {
                { "aud", GetAssertionAud(environment) },
                { "scope", scope },
                { "iss", clientId },
                { "exp", dateTimeOffset.ToUnixTimeSeconds() + 10 },
                { "iat", dateTimeOffset.ToUnixTimeSeconds() },
                { "jti", Guid.NewGuid().ToString() },
            };

            if (!string.IsNullOrEmpty(resource))
            {
                payload.Add("resource", resource);
            }

            JwtSecurityToken securityToken = new JwtSecurityToken(header, payload);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(securityToken);
        }

        private async Task<TokenResponse> GetToken(X509Certificate2 cert, JsonWebKey jwk, string environment, string clientId, string scope, string resource, bool disableCaching)
        {
            string cacheKey = $"{clientId}-{scope}-{resource}";

            await SemaphoreSlim.WaitAsync();
            try
            {
                if (!disableCaching)
                {
                    (bool hasCachedValue, TokenResponse cachedTokenResponse) = await _tokenCacheProvider.TryGetToken(cacheKey);
                    if (hasCachedValue)
                    {
                        return cachedTokenResponse;
                    }
                }

                string jwtAssertion = GetJwtAssertion(cert, jwk, environment, clientId, scope, resource);
                HttpRequestMessage requestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(GetTokenEndpoint(environment)),
                    Content = GetUrlEncodedContent(jwtAssertion)
                };

                TokenResponse accesstokenResponse = await PerformRequest<TokenResponse>(requestMessage);
                await _tokenCacheProvider.Set(cacheKey, accesstokenResponse,
                    new TimeSpan(0, 0, Math.Max(0, accesstokenResponse.ExpiresIn - 5)));
                return accesstokenResponse;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
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

        public async Task<T> PerformRequest<T>(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                string successResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(successResponse);
            }
            
            string errorResponse = await response.Content.ReadAsStringAsync();
            ErrorReponse error;
            try
            {
                error = JsonSerializer.Deserialize<ErrorReponse>(errorResponse);
            }
            catch (JsonException)
            {
                error = new ErrorReponse
                {
                    ErrorType = "Other",
                    Description = "An error occured, received from server: " +
                                  (string.IsNullOrEmpty(errorResponse) ? "<empty>" : errorResponse)
                };
            }

            _logger.LogError("errorType={errorType} description={description} statuscode={statusCode}", error!.ErrorType, error.Description, response.StatusCode);
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

        private string GetTokenExchangeEndpoint(string environment)
        {
            return environment switch
            {
                "prod" => "https://platform.altinn.no/authentication/api/v1/exchange/maskinporten",
                "ver1" => "https://platform.tt02.altinn.no/authentication/api/v1/exchange/maskinporten",
                "ver2" => "https://platform.tt02.altinn.no/authentication/api/v1/exchange/maskinporten",
                _ => throw new ArgumentException("Invalid environment setting. Valid values: prod, ver1, ver2")
            };
        }
    }
}
