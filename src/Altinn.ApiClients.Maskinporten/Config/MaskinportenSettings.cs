using Altinn.ApiClients.Maskinporten.Interfaces;

namespace Altinn.ApiClients.Maskinporten.Config
{
    public class MaskinportenSettings
    {
        /// <summary>
        /// ClientID to use
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Scopes to request. Must be provisioned on the supplied client.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Resource claim for assertion. This will be the `aud`-claim in the received access token
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The Maskinporten environment. Valid values are ver1, ver2 or prod
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Path to X.509 certificate with private key in PKCS#12-file 
        /// </summary>
        public string CertificatePkcs12Path { get; set; }

        /// <summary>
        /// Secrete to X.509 certificate with private key in PKCS#12-file 
        /// </summary>
        public string CertificatePkcs12Password { get; set; }

        /// <summary>
        /// Thumbprint for cert in local machine certificate store (Windows only)
        /// </summary>
        public string CertificateStoreThumbprint { get; set; }

        /// <summary>
        /// Base64 Encoded Json Web Key 
        /// </summary>
        public string EncodedJwk { get; set; }

        /// <summary>
        /// Optional enterprise username for token enrichment
        /// </summary>
        public string EnterpriseUserName { get; set; }

        /// <summary>
        /// Optional enterprise password for token enrichment
        /// </summary>
        public string EnterpriseUserPassword { get; set; }

        /// <summary>
        /// Optional. Enables Altinn token exchange without enterprise user authentication. Ignored if EnterpriseUserName/Password is supplied (which implies token exchange).
        /// </summary>
        public bool? ExhangeToAltinnToken { get; set; }

        /// <summary>
        /// Optional. Enables the DigDir token to be exchanged into a Altinn token for the test organisation.
        /// </summary>
        public bool? UseAltinnTestOrg { get; set; }
    }

    public class MaskinportenSettings<T> : MaskinportenSettings where T : IClientDefinition { }
}
