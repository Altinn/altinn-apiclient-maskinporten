using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public interface IMaskinporten
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
    }
}
