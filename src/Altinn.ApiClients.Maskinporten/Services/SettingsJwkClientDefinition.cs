using System;
using System.Text;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;
using Microsoft.IdentityModel.Tokens;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public class SettingsJwkClientDefinition : IClientDefinition
    {
        public IMaskinportenSettings ClientSettings { get; set; }

        public async Task<ClientSecrets> GetClientSecrets()
        {
            ClientSecrets clientSecrets = new ClientSecrets();

            if (!string.IsNullOrEmpty(ClientSettings.EncodedJwk))
            {
                byte[] base64EncodedBytes = Convert.FromBase64String(ClientSettings.EncodedJwk);
                string jwkjson = Encoding.UTF8.GetString(base64EncodedBytes);
                clientSecrets.ClientKey = new JsonWebKey(jwkjson);
            }

            return await Task.FromResult(clientSecrets);
        }
    }
}