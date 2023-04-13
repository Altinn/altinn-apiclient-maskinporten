using Altinn.ApiClients.Maskinporten.Interfaces;

namespace Altinn.ApiClients.Maskinporten.Config
{
    public class MaskinportenSettings : IMaskinportenSettings
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
        /// The Maskinporten environment. Valid values are ver1, ver2, test or prod
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Path to X.509 certificate with private key in PKCS#12-file 
        /// </summary>
        public string CertificatePkcs12Path { get; set; }

        /// <summary>
        /// Secret to X.509 certificate with private key in PKCS#12-file
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
        /// Base64 Encoded X509 certificate with private key 
        /// </summary>
        public string EncodedX509 { get; set; }

        /// <summary>
        /// Optional. Consumer organization number for Maskinporten-based delegations
        /// </summary>
        public string ConsumerOrgNo { get; set; }

        /// <summary>
        /// Optional. Enterprise username for token enrichment
        /// </summary>
        public string EnterpriseUserName { get; set; }

        /// <summary>
        /// Optional. Enterprise password for token enrichment
        /// </summary>
        public string EnterpriseUserPassword { get; set; }

        /// <summary>
        /// Optional. Enables Altinn token exchange without enterprise user authentication. Ignored if EnterpriseUserName/Password is supplied (which implies token exchange).
        /// </summary>
        public bool? ExhangeToAltinnToken { get; set; }

        /// <summary>
        /// Optional. The Altinn Token Exchange environment. Valid values are prod, tt02, at21, at22, at23, at24. Default: derive from 'Environment'
        /// </summary>
        public string TokenExchangeEnvironment { get; set; }

        /// <summary>
        /// Optional. Enables the DigDir token to be exchanged into a Altinn token for the test organisation.
        /// </summary>
        public bool? UseAltinnTestOrg { get; set; }

        /// <summary>
        /// Optional. Enabels verbose logging that should only be enabled when troubleshooting. Will cause logging (with severity "Information") of assertions.  
        /// </summary>
        public bool? EnableDebugLogging { get; set; }

        /// <summary>
        /// Optional. Overwrites existing Authorization-header if set. Default: ignore existing Authorization-header.
        /// </summary>
        public bool? OverwriteAuthorizationHeader { get; set; }
    }
}
