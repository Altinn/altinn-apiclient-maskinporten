using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleWebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Explicitly add a file based token cache store. You could add your own by implementing ITokenCacheProvider.
// This will place a file called ".maskinportenTokenCache.json" in the users temp-folder. This will NOT be deleted
// after application termination. This makes it suitable for CLI usage.
//
// If no token cache store is added before the first AddMaskinportenHttpClient-call, a MemoryCache-based cache store
// will be used.
builder.Services.AddSingleton<ITokenCacheProvider, FileTokenCacheProvider>();

// Specifying a separate HttpClient type and a HttpClient implementation type is optional. If you don't specify the
// client implementation type, the client type will be used as implementation type. This uses
// AddHttpClient<TClient, TImplementation>() under the hood.
builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
    builder.Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

// Dedicated sample client for system user requests. This uses request-scoped authorization_details and exchanges the
// received Maskinporten token to an Altinn token before the outbound request is sent.
builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MySystemUserMaskinportenHttpClient>(
    builder.Configuration.GetSection("MaskinportenSettingsForSystemUserExample"),
    clientDefinition => { clientDefinition.ClientSettings.ExhangeToAltinnToken = true; });

// Additional sample registrations are left below for reference if you want to try other client definition variants.
/*
builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, IMyMaskinportenHttpClient, MyMaskinportenHttpClient>(
    builder.Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

var maskinportenSettingsForSomeExternalApi = new MaskinportenSettings();
builder.Configuration.GetSection("MaskinportenSettingsForSomeExternalApi").Bind(maskinportenSettingsForSomeExternalApi);
builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, IMyMaskinportenHttpClient, MyMaskinportenHttpClient>(
    maskinportenSettingsForSomeExternalApi);

builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, IMyOtherMaskinportenHttpClient, MyOtherMaskinportenHttpClient>(
    builder.Configuration.GetSection("MaskinportenSettingsForSomeOtherExternalApi"));

builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyThirdMaskinportenHttpClient>(
    builder.Configuration.GetSection("MaskinportenSettingsForSomeOtherExternalApi"), clientDefinition =>
    {
        clientDefinition.ClientSettings.ExhangeToAltinnToken = true;
        clientDefinition.ClientSettings.Scope =
            "altinn:serviceowner/instances.read altinn:serviceowner/instances.write";
    });

builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient1", maskinportenSettingsForSomeExternalApi);
builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient2",
    builder.Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));
builder.Services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient3",
    builder.Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

var myCustomClientDefinitionSettings = new MyCustomClientDefinitionSettings();
builder.Configuration.GetSection("MyCustomClientDefinition").Bind(myCustomClientDefinitionSettings);
builder.Services.AddMaskinportenHttpClient<MyCustomClientDefinition, MyFourthMaskinportenHttpClient>(
    myCustomClientDefinitionSettings);
builder.Services.AddMaskinportenHttpClient<MyCustomClientDefinition>("myhttpclient4", myCustomClientDefinitionSettings);
*/

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
