Sample project README

Minimal working version
- Add the downstream echo URL to `appsettings.Development.json` or user secrets:
```json
{
  "SampleWebApp:TargetUrl": "https://your-pipedream-url"
}
```
- Add secrets for the plain Maskinporten sample:
```json
{
  "MaskinportenSettingsForSomeExternalApi:EncodedJwk": "Jwk",
  "MaskinportenSettingsForSomeExternalApi:ClientId": "client id",
  "MaskinportenSettingsForSomeExternalApi:Scope": "digdir scope that match Client id"
}
```
- Add secrets for the system user + Altinn exchange sample:
```json
{
  "MaskinportenSettingsForSystemUserExample:EncodedJwk": "Jwk",
  "MaskinportenSettingsForSystemUserExample:Kid": "maskinporten key identifier",
  "MaskinportenSettingsForSystemUserExample:ClientId": "client id",
  "MaskinportenSettingsForSystemUserExample:Scope": "scope with system user and exchange access"
}
```

Endpoints
- `GET /MaskinportenTest`
  - Uses the basic typed client flow with the `MaskinportenSettingsForSomeExternalApi` section.
  - Sends the request to the configured `SampleWebApp:TargetUrl`.
- `GET /MaskinportenTest/systemuser?customerOrgNo=212485772`
  - Uses request-scoped `authorization_details` with `urn:altinn:systemuser`.
  - Uses the dedicated `MaskinportenSettingsForSystemUserExample` section with JWK + `kid`.
  - Exchanges the Maskinporten token to an Altinn token before sending the downstream request.
  - Returns the downstream status and response body from the configured `SampleWebApp:TargetUrl`.

Notes
- `customerOrgNo` is supplied per request to demonstrate request-scoped system user context.
- `externalRef` can be supplied as an optional query parameter.
- The sample expects the target URL to be an echo endpoint such as Pipedream.
- `Kid` must match the key identifier registered for the client in Maskinporten.
