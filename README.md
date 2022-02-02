# .NET client for Maskinporten APIs

This .NET client library is used for calling maskinporten and create an access token to be used for services that require an Maskinporten access token. This also supports exchanging Maskinporten tokens into Altinn tokens, as well as enriching tokens with enterprise user credentials.

## Installation

Install the nuget with `dotnet add package Altinn.ApiClients.Maskinporten` or similar.

Pre-release versions of this nuget are made available on Github.

## Using extension methods to configure a HttpClient

There are different ways to set this up. For most using the extensions methods is the most convenient way of using this library, offering a HttpClient that
can be injected and used transparently.

You will need to configure a client definition, which is a way of providing the necessary OAuth2-related settings (client-id, scopes etc) as well as a way of getting
the secret (either a X.509 certificate with a private key or a JWK with a private key) used to sign the requests to Maskinporten.

There are several different client definitions available, or one can provide a custom one if required. See the "SampleWebApp"-project (especially Startup.cs) for examples
on how this can be done.

1. Client needs to configured in `ConfigureServices`, where `services` is a `IServiceCollection`

Here is an example with a [named client](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#implement-your-typed-client-classes-that-use-the-injected-and-configured-httpclient) using a client definition where the secret is a  private RSA key in a JWK supplied in the injected settings.

```c#
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(
    Configuration.GetSection("MaskinportenSettings"),
    "myhttpclient");
```
Another example using a client definition where the secret is a X509 enterprise certificate placed in a PKCS#12 file on disk.
```c#
services.AddMaskinportenHttpClient<Pkcs12ClientDefinition>(
    Configuration.GetSection("MyMaskinportenSettingsForCertFile"),
    "myhttpclient");
```
You can for all client definitions opt to use a [typed client](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#implement-your-typed-client-classes-that-use-the-injected-and-configured-httpclient) instead of a named client:
```c#
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
    Configuration.GetSection("MaskinportenSettings"));
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
    private readonly MyMaskinportenHttpClient _myMaskinportenHttpClient;

    public MyController(
        IHttpClientFactory clientFactory, MyMaskinportenHttpClient myMaskinportenHttpClient)
     {
        _clientFactory = clientFactory;
        _myMaskinportenHttpClient = myMaskinportenHttpClient;
     }

    [HttpGet]
    public async Task<string> Get()
    {
       // Here we use the named client we configured earlier
       var myclient = _clientFactory.CreateClient("myhttpclient");

       // This request will be sent with a Authorization-header containing a bearer token
       var result = await client.GetAsync("https://example.com/");

       // Or we can use the typed client we made instead. Any
       // requests made to the HttpClient instance injected weill have a bearer token.
       _myMaskinportenHttpClient.DoStuff();
    }
}
```

## Using Altinn token exchange

If you require an [Altinn Exchanged token](https://docs.altinn.studio/altinn-api/authentication/#maskinporten-jwt-access-token-input), this can be performed transparently by
supplying the following field to the settings object.

```json
"ExhangeToAltinnToken": true
````
This will transparently exchange (and cache) the Maskinporten-token into an Altinn-token which can be used against Altinn APIs.

If you require an Altinn Exchanged token for the TTD organisation, this is supported by including the field below in the settings.

```json
"UseAltinnTestOrg": true
```

## Authenticating with a enterprise user

This library also supports enriching Maskinporten tokens with enterprise user credentials for APIs requiring user roles/rights. In order to do this, you will need to add the following fields to the configuration, containing the enterpriseuser's username and password.

```json
"EnterpriseUserName": "myenterpriseuser",
"EnterpriseUserPassword": "mysecret",
````

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
