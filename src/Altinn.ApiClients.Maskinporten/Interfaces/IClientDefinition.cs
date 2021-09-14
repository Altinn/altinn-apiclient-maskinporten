using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Models;

namespace Altinn.ApiClients.Maskinporten.Interfaces
{
    public interface IClientDefinition
    {
        MaskinportenSettings ClientSettings { get; set; }
        Task<ClientSecrets> GetClientSecrets();
    }

    public interface IClientDefinition<T> : IClientDefinition where T : IClientDefinition { }
}
