using DemoWebService.Extensions;
using DemoWebService.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoWebService
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
            services.AddOptions()
                .AddResponseCompression()
                .AddMemoryCache();

            services.ConfigureRatelimiting(Configuration)
                .ConfigureCors()
                .ConfigureComponents(Configuration)
                .ConfigureSwagger()
                .ConfigureMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsProduction())
            {
                app.UseHsts();
            }

            appLifetime.ApplicationStarted.Register(() => 
                app.ApplicationServices.GetRequiredService<IAutoReloadingResolver>().Initialize()
            );

            app.UseHttpsRedirection()
                .UseResponseCompression()
                .UseRateLimiting()
                .UseSwaggerExt()
                .UseMvc();
        }
    }
}
