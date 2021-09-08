# .NET client for Maskinporten APIs


This .NET client library is used for calling maskinporten and create an access token to be used for services that require an Maskinporten access token

## Setup as HttpHandler with config in Appsettings

There are different ways to set this up. For most using the HttpHandler configuration is easyies

1. Client needs to configured in startup

```c#
    // Maskinporten setup with custom API client setup
    services.Configure<MaskinportenSettings>(Configuration.GetSection("MaskinportenSettings"));
    services.AddTransient<IClientSecret, AltinnAppClientSecretService>();
    services.AddHttpClient<IMaskinporten, MaskinportenService>();
    services.AddTransient<MaskinportenTokenHandler>();
    services.AddHttpClient<IReelleRettigheter, ReelleRettigheter>().AddHttpMessageHandler<MaskinportenTokenHandler>();
```

2. Configure Maskinporten environement in appsetting.json

```json
  "MaskinportenSettings": {
    "Environment": "ver2",
    "ClientId": "e15abbbc-36ad-4300-abe9-021c9a245e20",
    "Scope": "altinn:serviceowner/readaltinn",
    "EncodedJwk": "eyJwIjoiMms2RlZMRW9iVVY0dmpjRjRCVWNLOUhasdfasdfarhgawfN2YXE5eE95a3NyS1Q345435S19oNV45645635423545t45t54wrgsdfgsfdgsfd444aefasdf5NzdFcWhGTGtaSVAzSmhZTlA0MEZOc1EifQ=="
  }
```

## Setup as Http Hander with custom Secret Service


There are different ways to set this up. For most using the HttpHandler configuration is easyies

1. Client needs to configured in startup

```c#
    // Maskinporten setup with custom API client setup
    services.Configure<MaskinportenSettings>(Configuration.GetSection("MaskinportenSettings"));
    services.AddTransient<IClientSecret, AltinnAppClientSecretService>();
    services.AddHttpClient<IMaskinporten, MaskinportenService>();
    services.AddTransient<MaskinportenTokenHandler>();
    services.AddHttpClient<IReelleRettigheter, ReelleRettigheter>().AddHttpMessageHandler<MaskinportenTokenHandler>();
```

2. Configure Maskinporten environement in appsetting.json

```json
  "MaskinportenSettings": {
    "Environment": "ver2",
    "ClientId": "e15abbbc-36ad-4300-abe9-021c9a245e20",
    "Scope": "altinn:serviceowner/readaltinn",
  }
```

3. Create custom implementation of SecretService.

```c#
namespace Altinn.App.Services
{
    /// <summary>
    /// Custom implementation of ClientSecret Service. Get Json Webkey from Azure Keyvault throug Altinn App Secret Service
    /// </summary>
    public class AltinnAppClientSecretService : IClientSecret
    {
        private ISecrets _secrets;

        public AltinnAppClientSecretService(ISecrets secrets)
        {
            _secrets = secrets;
        }

        public async Task<ClientSecrets> GetClientSecrets()
        {
            ClientSecrets clientSecrets = new ClientSecrets();

            string base64encodedJWK = await _secrets.GetSecretAsync("maskinportentoken");
            byte[] base64EncodedBytes = Convert.FromBase64String(base64encodedJWK);
            string jwkjson = Encoding.UTF8.GetString(base64EncodedBytes);
            clientSecrets.ClientKey = new JsonWebKey(jwkjson);

            return clientSecrets;
        }
    }
}
```

## Manual use of TokenService

1. Client needs to configured in startup

```c#
    // Maskinporten setup with custom API client setup
    services.Configure<MaskinportenSettings>(Configuration.GetSection("MaskinportenSettings"));
    services.AddTransient<IClientSecret, AltinnAppClientSecretService>();
    services.AddHttpClient<IMaskinporten, MaskinportenService>();
    services.AddHttpClient<IReelleRettigheter, ReelleRettigheter>().AddHttpMessageHandler<MaskinportenTokenHandler>();
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