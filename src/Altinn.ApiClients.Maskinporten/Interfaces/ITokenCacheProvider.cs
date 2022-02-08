using System;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Models;

namespace Altinn.ApiClients.Maskinporten.Interfaces
{
    public interface ITokenCacheProvider
    {
        public Task<(bool success, TokenResponse result)> TryGetToken(string key);
        public Task Set(string key, TokenResponse value, TimeSpan timeToLive);
    }
}