# .NET client for Maskinporten APIs

This .NET client library is used for calling maskinporten and create an access token to be used for services that require an Maskinporten access token

## Installation

Install the nuget with `dotnet add package Altinn.ApiClients.Maskinporten` or similar.

Pre-release versions of this nuget are made available on Github.

## Setup as HttpHandler 

There are different ways to set this up. For most using the HttpHandler configuration is the most convenient way of using this library, offering a HttpClient that
can be injected and used transparently.

You will need to configure a client definition, which is a way of providing the necessary OAuth2-related settings (client-id, scopes etc) as well as a way of getting 
the secret (either a X.509 certificate with a private key or a JWK with a private key) used to sign the requests to Maskinporten. 

There are several different client definitions available, or one can provide a custom one if required. See the "SampleWebApp"-project (especially Startup.cs) for examples
on how this can be done.

1. Client needs to configured in `ConfigureServices`, where `services` is a `IServiceCollection`

```c#
// Maskinporten requires a memory cache implementation
services.AddSingleton<IMemoryCache, MemoryCache>();

// We also need at least one HTTP client in order to fetch tokens
services.AddHttpClient();

// We only need a single Maskinporten-service for all Maskinporten-powered clients. This service can be used directly if low level access is required (see below).
services.AddSingleton<IMaskinportenService, MaskinportenService>();

// Add the configuration needed for the client definition you want to use
services.Configure<MaskinportenSettings<SettingsJwkClientDefinition>>(Configuration.GetSection("MaskinportenSettings"));

// Add the client definitition you want to use. In this one we have a base64-encoded JWK defined in the settings.
services.AddSingleton<SettingsJwkClientDefinition>();   

// Add handler for the client definition
services.AddTransient<MaskinportenTokenHandler<SettingsJwkClientDefinition>>();
            
// Add a named (or typed) client
services.AddHttpClient("myclient").AddHttpMessageHandler<MaskinportenTokenHandler<SettingsJwkClientDefinition>>();
```

2. Configure Maskinporten environment in appsetting.json

```json
  "MaskinportenSettings": {
    "Environment": "ver2",
    "ClientId": "e15abbbc-36ad-4300-abe9-021c9a245e20",
    "Scope": "altinn:serviceowner/readaltinn",
    "EncodedJwk": "eyJwIjoiMms2RlZMRW9iVVY0dmpjRjRCVWNLOUhasdfasdfarhgawfN2YXE5eE95a3NyS1Q345435S19oNV45645635423545t45t54wrgsdfgsfdgsfd444aefasdf5NzdFcWhGTGtaSVAzSmhZTlA0MEZOc1EifQ=="
  }
```

3. Using the client

```c#
// The Maskinporten-enabled client can then be utilized like any other HttpClient via HttpClientFactory, eg DI-ed in a controller like this:
public class MyController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;

    public MyController(IHttpClientFactory clientFactory)
     {
        _clientFactory = clientFactory;
     }

    [HttpGet]
    public async Task<string> Get()
    {
       var myclient = _clientFactory.CreateClient("myclient");
       
       // This request will be sent with a Authorization-header containing a bearer token
       var result = await client.GetAsync("https://example.com/");
    }
}
```

**See the "SampleWebApp"-project (especially Startup.cs) for more examples on various client defintions, custom definitions and several clients with different configurations**

## Manual use of TokenService

1. Client needs to configured in startup

```c#
// Maskinporten requires a memory cache implementation
services.AddSingleton<IMemoryCache, MemoryCache>();

// We also need at least one HTTP client in order to fetch tokens
services.AddHttpClient();

// Adds service can be used directly, see below. This exposes several GetToken() overloads.
services.AddSingleton<IMaskinportenService, MaskinportenService>();
```

3. Configure client in constructur for service that need Maskinporten token

4. Call maskinporten API to get the token. Here we use MemoryCache ourselves to cache the token based on its exp-claim.

```c#
private async Task<string> GetMaskinPortenAccessToken()
{
    try
    {
        string accessToken = null;

        string cacheKey = $"{_maskinportenSettings.ClientId}-{_maskinportenSettings.Scope}";

        if (!_memoryCache.TryGetValue(cacheKey, out accessToken))
        {
            string base64encodedJWK = await _secrets.GetSecretAsync("myBase64EncodedJwkWithPrivateKey");
            TokenResponse accesstokenResponse = await _maskinporten.GetToken(base64encodedJWK, _maskinportenSettings.ClientId, _maskinportenSettings.Scope, null);
            accessToken = accesstokenResponse.AccessToken;
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.High,
            };
            cacheEntryOptions.SetAbsoluteExpiration(new TimeSpan(0, 0, accesstokenResponse.ExpiresIn - 10));
            _memoryCache.Set(cacheKey, accessToken, cacheEntryOptions);
        }

        return accessToken;
    }
}
```
