# .NET client for Maskinporten APIs

This .NET client library is used for calling maskinporten and create an access token to be used for services that require an Maskinporten access token. This also supports exchanging Maskinporten tokens into Altinn tokens, as well as enriching tokens with enterprise user credentials. 

## Installation

Install the nuget with `dotnet add package Altinn.ApiClients.Maskinporten` or similar.

Pre-release versions of this nuget are made available on Github.

## Usage

This library provides extensions methods providing means to configure one or more HttpClients that can be injected and used transparently as any other HttpClient instance.

You will need to configure a client definition, which is a way of providing the necessary OAuth2-related settings (client-id, scopes etc), as well as a way of getting the secret (either a X.509 certificate with a private key or a JWK with a private key) used to sign the requests to Maskinporten. The client definition also contains other settings, such as whether Altinn token exchange should be used.

> Note: There are several different client definition types built-in that can be used for aquiring secrets from various, or one can provide a custom one if required. It is also possible to create several named/typed clients using different combinations of settings and definition types. See below for a list of builtin client definitions, and the "SampleWebApp"-project (especially Startup.cs) for examples on how this can be done and extended with your own custom definitions if required.

Here is an example with a both a [named](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#named-clients) and [typed](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#typed-clients) client using a client definition where the secret is a private RSA key in a JWK supplied in the injected settings.

1. Client needs to configured in `ConfigureServices`, where `services` is a `IServiceCollection`

```c#
// Named client
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient",
  Configuration.GetSection("MaskinportenSettings"));

// Typed client (MyMaskinportenHttpClient is any class accepting a HttpClient paramter in its constructor)
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
  Configuration.GetSection("MaskinportenSettings")); 

// Another typed client, using the same app settings, but overriding the setting for Altinn token exchange
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
  Configuration.GetSection("MaskinportenSettings"), clientDefinition =>
{
    clientDefinition.ClientSettings.ExhangeToAltinnToken = true;
});

// You can chain additional handlers or configure the client if required 
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
  Configuration.GetSection("MaskinportenSettings"))
    .AddHttpMessageHandler(sp => ...)
    .ConfigureHttpClient(client => ...)
            
// Registering av Maskinporten-powered client without adding it to HttpClientFactory / DIC
services.RegisterMaskinportenClientDefinition<SettingsJwkClientDefinition>(
  "my-client-definition-instance-key",
  Configuration.GetSection("MaskinportenSettings"));

// This can then be added as a HttpMessageHandler to any IClientBuilder. This is
// useful if you're already using a client builder (DAN, Polly, Refit etc).
services.AddHttpClient<MyMaskinportenHttpClient>()
    .AddMaskinportenHttpMessageHandler<SettingsJwkClientDefinition>("my-client-definition-instance-key");


```
2. Configure Maskinporten environment in appsetting.json

```jsonc

  // Settings from appsettings.json, environment variables or other configuration providers.
  // The first three are always mandatory for all client definitions types
  "MaskinportenSettings": {
    // 1. Valid values are ver1, ver2 and prod
    "Environment": "ver2", 

    // 2. Client Id/integration as configured in Maskinporten
    "ClientId": "e15abbbc-36ad-4300-abe9-021c9a245e20", 
    
    // 3. Scope(s) requested, space seperated. Must be provisioned on supplied client id.
    "Scope": "altinn:serviceowner/readaltinn", 

    // --------------------------
    // Any additional settings are specific for the selected client definition type. 
    // See below for examples using other types.
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
       // requests made to the HttpClient instance injected will have a bearer token.
       _myMaskinportenHttpClient.DoStuff();
    }
}
```
## Built-in client definitions

| Name             | Description
| -----------------| -------------
| SettingsJwk      | Uses a Base64-encoded RSA keypair in a JWK supplied in injected settings
| SettingsX509     | Uses a Base64-encoded X.509 certificate with a private key supplied in injected settings
| Pkcs12           | Uses a password-protected PKCS#12 formatted certificate file on disk
| CertificateStore | Uses a thumbprint in Windows Certificate Store (LocalMachine\My)

<details><summary>See examples using the various client definition types</summary>

Below are usage examples. Note that configuration binding (ie. `Configuration.GetSection("somesection")`) is omitted, as are the three mandatory settings that must always be supplied (Environment, ClientId, Scope)

### SettingsJwk

```c#
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>( ... )
```

```jsonc
  "MaskinportenSettings": {
    // ...
    "EncodedJwk": "eyJwIjoiMms2RlZMRW9iV..."
  }
```

### SettingsX509

```c#
services.AddMaskinportenHttpClient<SettingsX509ClientDefinition>( ... )
```

```jsonc
  "MaskinportenSettings": {
    // ...
    "EncodedX509": "MIIwIjoiMms2RlZMRW9i..."
  }
```

### Pkcs12

```c#
services.AddMaskinportenHttpClient<Pkcs12ClientDefinition>( ... )
```

```jsonc
  "MaskinportenSettings": {
    // ...
    "CertificatePkcs12Path": "Certs/mycert.p12",
    "CertificatePkcs12Password": "mysecretpassword",
  }
```

### CertificateStore

```c#
services.AddMaskinportenHttpClient<Pkcs12ClientDefinition>( ... )
```

```jsonc
  "MaskinportenSettings": {
    // ...
    "CertificateStoreThumbprint": "4325B22433984608AB5049103837F11C6BCA520D",
  }
```
</details>

## Using with Azure Keyvault as configuration provider i Azure App Services

JWKs or certificates can be injected into application settings for Azure App Services or Azure Functions using [key vault references](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references?tabs=azure-cli). This can then be easily used with the `SettingsJwk` or `SettingsX509` client definitions.

Given that your applications managed identity has access to the key vault containing the secret/cert, you can specify the appsetting value like this:

```jsonc
"MaskinportenSettings": {
    // ...
    "EncodedJwk": "@Microsoft.KeyVault(VaultName=myvault;SecretName=mysecretjwk)"
  }
```
or for certificates:

```jsonc
"MaskinportenSettings": {
    // ...
    "EncodedX509": "@Microsoft.KeyVault(VaultName=myvault;SecretName=mycertificate)"
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

The environment for the token exchange is by default derived from the Maskinporten environment. This can also be explicitly configured:

```jsonc
// Valid values: at21, at22, at23, at24, tt02, prod
"TokenExchangeEnvironment": "at24"
```

These settings can also be supplied by providing a delegate like:

```c#
services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
  Configuration.GetSection("MaskinportenSettings"), clientDefinition =>
{
    clientDefinition.ClientSettings.ExhangeToAltinnToken = true;
    clientDefinition.ClientSettings.UseAltinnTestOrg = true;
    clientDefinition.ClientSettings.TokenExchangeEnvironment = "at24";
});
```


## Authenticating with a enterprise user

This library also supports enriching Maskinporten tokens with enterprise user credentials for APIs requiring user roles/rights. In order to do this, you will need to add the following fields to the configuration, containing the enterpriseuser's username and password.

```json
"EnterpriseUserName": "myenterpriseuser",
"EnterpriseUserPassword": "mysecret",
````

## Custom cache provider (2.x and later)

By default, this library will cache tokens using MemoryCache via `MemoryCacheTokenProvider`, allowing tokens to be reused  as long as they are valid (based on `exp`-claim). If your application has other caching needs, you can provide your own implementation of `ITokenCacheProvider` by registering your implementation as a service before calling `AddMaskinportenHttpClient`.

```c#
services.AddSingleton<ITokenCacheProvider, FileTokenCacheProvider>();
```

`FileTokenCacheProvider` is included in the library, which uses a file based cache. 

**See the "SampleWebApp"-project (especially Startup.cs) for more examples on various client defintions, cache providers, custom definitions and several clients with different configurations**


## Manual use of TokenService

1. Client needs to configured in startup

```c#
// Maskinporten requires a cache implementation. Note: for 1.x of this library, use MemoryCache directly
services.AddSingleton<ITokenCacheProvider, MemoryTokenCacheProvider>();

// We also need at least one HTTP client in order to fetch tokens
services.AddHttpClient();

// Adds service can be used directly, see below. This exposes several GetToken() overloads.
services.AddSingleton<IMaskinportenService, MaskinportenService>();
```

3. Configure client in constructur for service that need Maskinporten token

4. Call maskinporten API to get the token. 

```c#
private async Task<string> GetMaskinportenAccessToken()
{
    try
    {
        string accessToken = null;
        string base64encodedJWK = await _secrets.GetSecretAsync("myBase64EncodedJwkWithPrivateKey");
        TokenResponse accesstokenResponse = await _maskinporten.GetToken(
            base64encodedJWK, _maskinportenSettings.ClientId, _maskinportenSettings.Scope, null);

        return accesstokenResponse.AccessToken;
    }
}
```

## Troubleshooting

When facing issues, you might want to temporarily enable debug logging in the settings by adding the following key:

```json
"EnableDebugLogging": true
```

This will cause various information to be logged with severity "Information" to the injected logger. All log entries will have the prefix `[Altinn.ApiClients.Maskinporten DEBUG]: `.
> Warning! This will cause signed assertions (a short-lived secret) to be logged, so only use this in troubleshooting scenarios. No private key material will be logged.
