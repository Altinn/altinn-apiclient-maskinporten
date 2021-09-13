using Altinn.ApiClients.Maskinporten.Interfaces;

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
        /// Resource if fixed
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
    }

    public class MaskinportenSettings<T> : MaskinportenSettings where T : IClientDefinition { }
}
