# .NET client for Maskinporten APIs




This .NET client library is used for calling maskinporten

## Requirements

1. Client needs to configured in startup

```c#
services.AddHttpClient<IMaskinporten, MaskinportenService>();
services.Configure<MaskinportenSettings>(Configuration.GetSection("MaskinportenSettings"));
```

2. Configure client in constructur for service that need Maskinporten token


3. Call maskinporten API and a

```c#
  private async Task<string> GetMaskinPortenAccessToken()
        {
            try
            {
                string base64encodedJWK = await _secrets.GetSecretAsync("maskinportentoken");
                string accesstoken = await _maskinporten.GetToken(base64encodedJWK, "eafasdfsad a245e20", "asdf:dsf", null);
                return accesstoken;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Not able to generate access token");
                throw;
            }
        }
```