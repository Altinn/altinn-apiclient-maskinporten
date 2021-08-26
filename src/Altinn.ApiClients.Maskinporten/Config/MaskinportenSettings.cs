using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.ApiClients.Maskinporten.Config
{
    public class MaskinportenSettings
    {
        /// <summary>
        /// The URI for the token endpoint
        /// </summary>
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// Set the Audience for the JWT Assertion
        /// </summary>
        public string JwtAssertionAudience { get; set; }
    }
}
