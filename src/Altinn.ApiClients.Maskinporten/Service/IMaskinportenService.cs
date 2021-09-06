using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Service;

namespace Altinn.ApiClients.Maskinporten.Services
{

    public interface IMaskinportenService<T> : IMaskinportenService where T : ICustomClientSecret { }

    public interface IMaskinportenService
    {
        /// <summary>
        /// Generates a Maskinporten access token using a JsonWebKey
        /// </summary>
        Task<TokenResponse> GetToken(JsonWebKey jwk, string clientId, string scope, string resource, bool disableCaching = false);

        /// <summary>
        /// Generates a Maskinporten access token using a X509Certificate
        /// </summary>
        Task<TokenResponse> GetToken(X509Certificate2 cert, string clientId, string scope, string resource, bool disableCaching = false);

        /// <summary>
        /// Generates a Maskinporten access token using a base64encoded JsonWebKey
        /// </summary>
        Task<TokenResponse> GetToken(string base64EncodedJWK, string clientId, string scope, string resource, bool disableCaching = false);

        /// <summary>
        /// Generates a access token based on configured values. Will take client secret from SecretService
        /// </summary>
        Task<TokenResponse> GetToken(bool disableCaching = false);
    }
}
