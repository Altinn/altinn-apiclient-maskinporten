using Altinn.ApiClients.Maskinporten.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Altinn.ApiClients.Maskinporten.Extensions;
using SampleWebApp.Config;

namespace SampleWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Explicitly add a file based token cache store. You could add your own by implementing ITokenCacheProvider.
            // This will place a file called ".maskinportenTokenCache.json" in the users temp-folder. This will NOT be deleted
            // after application termination. This makes it suitable for CLI usage.
            // 
            // If no token cache store is added before the first AddMaskinportenHttpClient-call, a MemoryCache-based cache store
            // will be used.
            services.AddSingleton<ITokenCacheProvider, FileTokenCacheProvider>();

            // Specifying separate HttpClient type and HttpClient implementation type, and passing IConfiguration instance which will be bound to an instance of MaskinportenSettings
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, IMyMaskinportenHttpClient, MyMaskinportenHttpClient>(
                Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

            // Specifying separat client type and client implementation type is optional. If you don't specify the client implementation type, the client type will be used as implementation type.
            // This uses AddHttpClient<TClient, TImplementation>() under the hood.
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
                Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

            // Using a typed HttpClient, and binding the MaskinportenSettings ourselves
            var maskinportenSettingsForSomeExternalApi = new MaskinportenSettings();
            Configuration.GetSection("MaskinportenSettingsForSomeExternalApi").Bind(maskinportenSettingsForSomeExternalApi);
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, IMyMaskinportenHttpClient, MyMaskinportenHttpClient>(maskinportenSettingsForSomeExternalApi);

            // If you need to access multiple APIs requiring different settings (ie. scopes) you must supply a unique combination of client type and client definition type.
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, IMyOtherMaskinportenHttpClient, MyOtherMaskinportenHttpClient>(
                Configuration.GetSection("MaskinportenSettingsForSomeOtherExternalApi"));

            // You can reuse application settings for the across different HTTP clients, but override specific settings
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyThirdMaskinportenHttpClient>(
                Configuration.GetSection("MaskinportenSettingsForSomeOtherExternalApi"), clientDefinition =>
                {
                    clientDefinition.ClientSettings.ExhangeToAltinnToken = true;
                    clientDefinition.ClientSettings.Scope =
                        "altinn:serviceowner/instances.read altinn:serviceowner/instances.write";
                });

            // As an alternative, named HTTP clients can be used. 
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient1", maskinportenSettingsForSomeExternalApi);
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient2", Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

            // Overloads are provided to send in a IConfiguration instance directly. This will be bound to an instance of MaskinportenSettings. 
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient3",
                Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));
            
            // You can also define your own client definitions with custom settings, which should inherit MaskinportenSettings (or implement IMaskinportenSettings)
            var myCustomClientDefinitionSettings = new MyCustomClientDefinitionSettings();
            Configuration.GetSection("MyCustomClientDefinition").Bind(myCustomClientDefinitionSettings);
            services.AddMaskinportenHttpClient<MyCustomClientDefinition, MyFourthMaskinportenHttpClient>(myCustomClientDefinitionSettings);

            // Named http clients for custom client definitions
            services.AddMaskinportenHttpClient<MyCustomClientDefinition>("myhttpclient4", myCustomClientDefinitionSettings);

            // You can chain additional handlers or configure the client further if you want
            /*
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(maskinportenSettingsForSomeExternalApi)
                .AddHttpMessageHandler(sp => ...)
                .ConfigureHttpClient(client => ...)
            */

            /*
            // Register a client definition and a configuration, identified by some arbitrary string key
            services.RegisterMaskinportenClientDefinition<SettingsJwkClientDefinition>("my-client-definition-key", maskinportenSettingsForSomeExternalApi);

            // This can then be added as a HttpMessageHandler to any IClientBuilder (if also using DAN, Polly, Refit etc)
            services.AddHttpClient<MyMaskinportenHttpClient>()
                    .AddMaskinportenHttpMessageHandler<SettingsJwkClientDefinition>("my-client-definition-key");
            */
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
