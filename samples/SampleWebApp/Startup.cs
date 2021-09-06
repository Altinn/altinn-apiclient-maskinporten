using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altinn.ApiClients.Maskinporten.Config;
using Altinn.ApiClients.Maskinporten.Handlers;
using Altinn.ApiClients.Maskinporten.Service;
using Altinn.ApiClients.Maskinporten.Services;
using Microsoft.Extensions.Caching.Memory;
using SampleWebApp.Service;

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

            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddHttpClient();

            // Add a configuration client
            services.Configure<MaskinportenSettings>(Configuration.GetSection("MaskinportenSettings"));
            services.AddSingleton<IClientSecret, ClientSecretService>();
            services.AddSingleton<IMaskinportenService, MaskinportenService>();
            services.AddSingleton<MaskinportenTokenHandler>();
            services.AddHttpClient("foo").AddHttpMessageHandler<MaskinportenTokenHandler>();
            
            // Add a separate client with custom config and secret
            services.Configure<MaskinportenSettings<IMyCustomClientSecretService>>(Configuration.GetSection("MyCustomMaskinportenSettings"));
            services.AddSingleton<IClientSecret<IMyCustomClientSecretService>, MyCustomClientSecretService>();
            services.AddSingleton<IMaskinportenService<IMyCustomClientSecretService>, MaskinportenService<IMyCustomClientSecretService>>();
            services.AddSingleton<MaskinportenTokenHandler<IMyCustomClientSecretService>>();
            services.AddHttpClient("bar").AddHttpMessageHandler<MaskinportenTokenHandler<IMyCustomClientSecretService>>();
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
