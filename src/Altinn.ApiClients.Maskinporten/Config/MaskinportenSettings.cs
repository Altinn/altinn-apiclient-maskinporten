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
        /// ClientID if fixed
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Scope if fixed
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// The Maskinporten environment. Valid values are ver1, ver2 or prod
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// The URI for the token endpoint
        /// </summary>
        public string TokenEndpointProd { get; set; } = "https://maskinporten.no/token";

        /// <summary>
        /// Set the Audience for the JWT Assertion
        /// </summary>
        public string JwtAssertionAudienceProd { get; set; } = "https://maskinporten.no/";

        /// <summary>
        /// The URI for the token endpoint
        /// </summary>
        public string TokenEndpointVer1 { get; set; } = "https://ver1.maskinporten.no/token";

        /// <summary>
        /// Set the Audience for the JWT Assertion
        /// </summary>
        public string JwtAssertionAudienceVer1 { get; set; } = "https://ver1.maskinporten.no/";

        /// <summary>
        /// The URI for the token endpoint
        /// </summary>
        public string TokenEndpointVer2 { get; set; } = "https://ver2.maskinporten.no/token";

        /// <summary>
        /// Set the Audience for the JWT Assertion
        /// </summary>
        public string JwtAssertionAudienceVer2 { get; set; } = "https://ver2.maskinporten.no/";
    }
}
