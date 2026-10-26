using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Models;
using Altinn.ApiClients.Maskinporten.Services;
using Xunit;

namespace Altinn.ApiClients.Maskinporten.Tests
{
    public class CertificateClientDefinitionTests
    {
        [NotOnMacOSTheory]
        [InlineData("secret")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Pkcs12ClientDefinition_LoadsCertificateWithPrivateKey(string password)
        {
            using X509Certificate2 certificate = CreateCertificate();
            string file = Path.Combine(Path.GetTempPath(), "altinn-apiclient-maskinporten-test-" + Guid.NewGuid());
            await File.WriteAllBytesAsync(file, certificate.Export(X509ContentType.Pkcs12, password));
            try
            {
                var clientDefinition = new Pkcs12ClientDefinition
                {
                    ClientSettings = new MaskinportenSettings
                    {
                        CertificatePkcs12Path = file,
                        CertificatePkcs12Password = password,
                    }
                };

                ClientSecrets clientSecrets = await clientDefinition.GetClientSecrets();

                Assert.Equal(certificate.Thumbprint, clientSecrets.ClientCertificate.Thumbprint);
                Assert.True(clientSecrets.ClientCertificate.HasPrivateKey);
            }
            finally
            {
                File.Delete(file);
            }
        }

        [NotOnMacOSTheory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SettingsX509ClientDefinition_LoadsCertificateWithPrivateKey(string password)
        {
            using X509Certificate2 certificate = CreateCertificate();
            var clientDefinition = new SettingsX509ClientDefinition
            {
                ClientSettings = new MaskinportenSettings
                {
                    EncodedX509 = Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12, password)),
                }
            };

            ClientSecrets clientSecrets = await clientDefinition.GetClientSecrets();

            Assert.Equal(certificate.Thumbprint, clientSecrets.ClientCertificate.Thumbprint);
            Assert.True(clientSecrets.ClientCertificate.HasPrivateKey);
        }

        private static X509Certificate2 CreateCertificate()
        {
            using RSA rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=altinn-apiclient-maskinporten-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddHours(1));
        }

        /// <summary>
        /// The client definitions load certificates with X509KeyStorageFlags.EphemeralKeySet, which is not supported on macOS.
        /// </summary>
        private sealed class NotOnMacOSTheoryAttribute : TheoryAttribute
        {
            public NotOnMacOSTheoryAttribute()
            {
                if (OperatingSystem.IsMacOS())
                {
                    Skip = "X509KeyStorageFlags.EphemeralKeySet is not supported on macOS";
                }
            }
        }
    }
}
