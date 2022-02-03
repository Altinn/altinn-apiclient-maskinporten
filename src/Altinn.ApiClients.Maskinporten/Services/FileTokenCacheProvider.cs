using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Models;

namespace Altinn.ApiClients.Maskinporten.Services
{
    public class FileTokenCacheProvider : ITokenCacheProvider
    {
        private readonly string _pathToCacheFile;
        private static Dictionary<string, TokenCacheStoreEntry> _tokenCacheStoreEntries = new();

        public FileTokenCacheProvider(string pathToCacheFile)
        {
            _pathToCacheFile = pathToCacheFile;
        }

        public FileTokenCacheProvider() : this(Path.GetTempPath() + ".maskinportenTokenCache.json") {}
        
        public async Task<(bool success, TokenResponse result)> TryGetToken(string key)
        {
            if (_tokenCacheStoreEntries.Count == 0)
            {
                await LoadTokenCacheStore();
            }

            if (_tokenCacheStoreEntries.TryGetValue(key, out TokenCacheStoreEntry cachedTokenEntry))
            {
                if (cachedTokenEntry.Expires > DateTime.UtcNow)
                {
                    return (true, cachedTokenEntry.TokenResponse);
                }
            }

            return (false, null);
        }

        public async Task Set(string key, TokenResponse value, TimeSpan timeToLive)
        {
            _tokenCacheStoreEntries[key] = new TokenCacheStoreEntry
            {
                TokenResponse = value,
                Expires = DateTime.UtcNow.Add(timeToLive)
            };

            await WriteTokenCacheStore();
        }

        private async Task LoadTokenCacheStore()
        {
            byte[] fileContents;
            await using (FileStream fs = File.Open(_pathToCacheFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (fs.Length == 0)
                {
                    return;
                }

                fileContents = new byte[fs.Length];
                await fs.ReadAsync(fileContents.AsMemory(0, (int)fs.Length));
            }

            _tokenCacheStoreEntries = JsonSerializer.Deserialize<Dictionary<string, TokenCacheStoreEntry>>(fileContents);
        }

        private async Task WriteTokenCacheStore()
        {
            // Remove all expired entries before writing
            foreach (var tokenCacheStoreEntry in 
                     _tokenCacheStoreEntries.Where(tokenCacheStoreEntry => tokenCacheStoreEntry.Value.Expires < DateTime.UtcNow))
            {
                _tokenCacheStoreEntries.Remove(tokenCacheStoreEntry.Key);
            }

            await using FileStream fs = File.Open(_pathToCacheFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.SetLength(0);
            await fs.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(_tokenCacheStoreEntries)));
            await fs.FlushAsync();
        }
    }

    internal class TokenCacheStoreEntry
    {
        public TokenResponse TokenResponse;
        public DateTime Expires;
    }
}