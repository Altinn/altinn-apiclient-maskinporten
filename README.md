# .NET client for Maskinporten APIs




This .NET client library is used for calling maskinporten

## Requirements

1. Client needs to configured in startup

```c#
services.AddHttpClient<IMaskinporten, MaskinportenService>();
services.Configure<MaskinportenSettings>(Configuration.GetSection("MaskinportenSettings"));
```

2. Configure Maskinporten environement in appsetting.json

```json
  "MaskinportenSettings": {
    "Environment": "ver2",
    "ClientId": "adsf-asdfasd--asf-asdf",
    "Scope": "altinn:instances"
  }
```

3. Configure client in constructur for service that need Maskinporten token


4. Call maskinporten API and a

```c#
    private async Task<string> GetMaskinPortenAccessToken()
        {
            try
            {
                string accessToken = null;

                string cacheKey = $"{_maskinportenSettings.ClientId}-{_maskinportenSettings.Scope}";

                if (!_memoryCache.TryGetValue(cacheKey, out accessToken))
                {
                    string base64encodedJWK = await _secrets.GetSecretAsync("maskinportentoken");
                    TokenResponse accesstokenResponse = await _maskinporten.GetToken(base64encodedJWK, _maskinportenSettings.ClientId, _maskinportenSettings.Scope, null);
                    accessToken = accesstokenResponse.AccessToken;
                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    {
                        Priority = CacheItemPriority.High,
                    };
                    cacheEntryOptions.SetAbsoluteExpiration(new TimeSpan(0, 0, accesstokenResponse.ExpiresIn - 30));
                    _memoryCache.Set(cacheKey, accessToken, cacheEntryOptions);
                 }

                return accessToken;

            }
```