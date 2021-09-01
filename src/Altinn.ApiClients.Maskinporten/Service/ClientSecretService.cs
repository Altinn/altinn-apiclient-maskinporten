using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Service
{
    public class ClientSecretService : IClientSecret
    {
        private MaskinportenSettings _maskinportenSettings;

        public ClientSecretService(IOptions<MaskinportenSettings> maskinportenSettings)
        {
            _maskinportenSettings = maskinportenSettings.Value;
        }

        public async Task<ClientSecrets> GetClientSecrets()
        {
            ClientSecrets clientSecrets = new ClientSecrets();

            if (!string.IsNullOrEmpty(_maskinportenSettings.EncodedJwk))
            {
                byte[] base64EncodedBytes = Convert.FromBase64String(_maskinportenSettings.EncodedJwk);
                string jwkjson = Encoding.UTF8.GetString(base64EncodedBytes);
                clientSecrets.ClientKey = new JsonWebKey(jwkjson);
            }

            return clientSecrets;
        }
    }
}
