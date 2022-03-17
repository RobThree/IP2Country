using AspNetCoreRateLimit;
using DemoWebService.Helpers;
using IP2Country;
using IP2Country.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DemoWebService
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions()
                .AddResponseCompression()
                .AddMemoryCache()
                .AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    o.JsonSerializerOptions.Converters.Add(new IPAddressConverter());
                });

            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IP Lookup", Version = "v1" });
                    c.MapType<IPAddress>(() => new OpenApiSchema { Type = typeof(string).Name });

                    // Set the comments path for the Swagger JSON and UI.
                    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
                });

            // Configure resolver
            services.Configure<AutoReloadingResolverConfig>(Configuration.GetSection("Resolver"));
            services.AddSingleton<IAutoReloadingResolver>(s =>
                new AutoReloadingResolver((ds) =>
                    new IP2CountryBatchResolver(
                        new CachingIP2CountryResolver(
                            new IP2CountryResolver(ds),
                            s.GetRequiredService<IMemoryCache>(),
                            TimeSpan.FromMinutes(10)
                        )
                    ),
                    s.GetRequiredService<IOptions<AutoReloadingResolverConfig>>()
                )
            );

            // IP Rate limiting
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // https://github.com/aspnet/Hosting/issues/793 the IHttpContextAccessor service is not registered by default. The clientId/clientIp resolvers use it.
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            if (env.IsProduction())
            {
                app.UseHsts();
            }

            app.ApplicationServices.GetRequiredService<IAutoReloadingResolver>().Initialize();

            app.UseIpRateLimiting()
                .UseHttpsRedirection()
                .UseResponseCompression()
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.DefaultModelExpandDepth(0);
                    c.DefaultModelsExpandDepth(0);

                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IP2Country V1");
                    //To serve the Swagger UI at the app's root (http://localhost:<port>/), set the RoutePrefix property to an empty string
                    //c.RoutePrefix = string.Empty;
                })
                .UseRouting()
                .UseCors()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}