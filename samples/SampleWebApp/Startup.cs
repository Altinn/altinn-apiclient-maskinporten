using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Altinn.ApiClients.Maskinporten.Interfaces;
using Altinn.ApiClients.Maskinporten.Services;
using Altinn.ApiClients.Maskinporten.Extensions;

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

            // Using a typed HttpClient is the preferred way of setting up a MaskinportenHttpClient;
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyMaskinportenHttpClient>(
                Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

            // If you need to access multiple APIs requiring different settings (ie. scopes) you must supply a different 
            // typed client (may inherit a common base client)
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyOtherMaskinportenHttpClient>(
                Configuration.GetSection("MaskinportenSettingsForSomeOtherExternalApi"));

            // You can reuse application settings for the across different HTTP clients, but override specific settings
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, MyThirdMaskinportenHttpClient>(
                Configuration.GetSection("MaskinportenSettingsForSomeOtherExternalApi"), clientDefinition =>
                {
                    clientDefinition.ClientSettings.ExhangeToAltinnToken = true;
                });

            // As an alternative, named HTTP clients can be used. 
            services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>("myhttpclient", 
                Configuration.GetSection("MaskinportenSettingsForSomeExternalApi"));

            // You can also define your own client definitions:
            services.AddMaskinportenHttpClient<MyCustomClientDefinition, MyFourthMaskinportenHttpClient>(
                Configuration.GetSection("MyCustomClientDefinition"), clientDefinition =>
                {
                    // Any additional custom settings and/or fields in your custom client definition should be populated in the configureClientDefinition delegate
                    Configuration.GetSection("MyCustomClientDefinition").Bind(clientDefinition.MyCustomClientDefinitionSettings);
                });

            // Named http clients for custom client definitions
            services.AddMaskinportenHttpClient<MyCustomClientDefinition>("myotherhttpclient", Configuration.GetSection("MyCustomClientDefinition"), clientDefinition =>
            {
                Configuration.GetSection("MyCustomClientDefinition").Bind(clientDefinition.MyCustomClientDefinitionSettings);
            });

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
