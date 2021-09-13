namespace Altinn.ApiClients.Maskinporten.Interfaces
{
    public interface IMaskinportenTokenHandler { }

    public interface IMaskinportenTokenHandler<T> : IMaskinportenTokenHandler where T : IClientDefinition { }
}
