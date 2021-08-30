using Altinn.ApiClients.Maskinporten.Config;
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

        public MaskinportenService(HttpClient httpClient, IOptions<MaskinportenSettings> maskinportenConfig)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = httpClient;
            _maskinportenConfig = maskinportenConfig.Value;
        }

     
        public async Task<string> GetToken(X509Certificate2 cert, string clientId, string scope, string resource, int exp)
        {
            return await GetToken(cert, null, clientId, scope, resource, exp);
        }

        public async Task<string> GetToken(JsonWebKey jwk, string clientId, string scope, string resource, int exp)
        {
            return await GetToken(null, jwk, clientId, scope, resource, exp);
        }

        public async Task<string> GetToken(string base64EncodedJwk, string clientId, string scope, string resource, int exp)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedJwk);
            string jwkjson = Encoding.UTF8.GetString(base64EncodedBytes);
            JsonWebKey jwk = new JsonWebKey(jwkjson);
            return await GetToken(null, jwk, clientId, scope, resource, exp);
        }


        private async Task<string> GetToken(X509Certificate2 cert, JsonWebKey jwk, string clientId, string scope, string resource, int exp)
        {
            string jwtAssertion = GetJwtAssertion(cert, jwk, clientId, scope, resource);
            FormUrlEncodedContent content = GetUrlEncodedContent(jwtAssertion);
            string maskinPortenToken = await PostToken(content);

            if (!string.IsNullOrEmpty(maskinPortenToken))
            {
                var accessTokenObject = JsonConvert.DeserializeObject<JObject>(maskinPortenToken);
                string accessToken = accessTokenObject.GetValue("access_token").ToString();
                return accessToken;
            }

            return null;
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

        public async Task<string> PostToken(FormUrlEncodedContent bearer)
        {
            string token = string.Empty;

            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(GetTokenEndpoint()),
                Content = bearer
            };

            HttpResponseMessage response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                token = await response.Content.ReadAsStringAsync();
                return token;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
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
