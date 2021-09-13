using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.IdentityModel.Tokens;

namespace Altinn.ApiClients.Maskinporten.Interfaces
{
    public interface IMaskinportenService
    {
        /// <summary>
        /// Generates a Maskinporten access token using a JsonWebKey
        /// </summary>
        Task<TokenResponse> GetToken(JsonWebKey jwk, string environment, string clientId, string scope, string resource, bool disableCaching = false);

        /// <summary>
        /// Generates a Maskinporten access token using a X509Certificate
        /// </summary>
        Task<TokenResponse> GetToken(X509Certificate2 cert, string environment, string clientId, string scope, string resource, bool disableCaching = false);

        /// <summary>
        /// Generates a Maskinporten access token using a base64encoded JsonWebKey
        /// </summary>
        Task<TokenResponse> GetToken(string base64EncodedJWK, string environment, string clientId, string scope, string resource, bool disableCaching = false);

        /// <summary>
        /// Generates a access token based on supplied definition containing settings and secrets.
        /// </summary>
        Task<TokenResponse> GetToken(IClientDefinition clientDefinition, bool disableCaching = false);
    }
}
