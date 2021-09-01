using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Models
{
    public class ClientSecrets
    {
        public JsonWebKey ClientKey { get; set; }

        public X509Certificate2 ClientCertificate { get; set; }
    }
}
