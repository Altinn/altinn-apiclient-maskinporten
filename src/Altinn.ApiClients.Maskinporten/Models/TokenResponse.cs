using System.Text.Json.Serialization;

namespace Altinn.ApiClients.Maskinporten.Models
{
    /// <summary>
    /// The TokenResponse from Maskinporten
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// An Oauth2 access token, either by reference or as a JWT depending on which scopes was requested and/or client registration properties.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        /// <summary>
        /// Number of seconds until this access_token is no longer valid
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// The list of scopes issued in the access token. Included for convenience only, and should not be trusted for access control decisions.
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Type of token
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}
