using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public interface IMaskinporten
    {
        Task<string> GetToken(JsonWebKey jwk, string clientId, string scope, string resource, int exp);

        Task<string> GetToken(X509Certificate2 cert, string clientId, string scope, string resource, int exp);

        Task<string> GetToken(string base64EncodedJWK, string clientId, string scope, string resource, int exp);
    }
}
