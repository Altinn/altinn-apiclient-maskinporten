namespace Altinn.ApiClients.Maskinporten.Models
{
    public class MaskinportenTokenRequestContext
    {
        /// <summary>
        /// Optional. System user authorization details to include in the token request.
        /// </summary>
        public SystemUserTokenRequest SystemUser { get; set; }
    }

    public class SystemUserTokenRequest
    {
        /// <summary>
        /// Organization number of the customer the system user token is requested for.
        /// Can be provided either as a plain org number or as a full ISO6523 identifier.
        /// </summary>
        public string OrganizationNumber { get; set; }

        /// <summary>
        /// Optional. External reference for the system user token request.
        /// </summary>
        public string ExternalReference { get; set; }
    }
}
