using DemoWebService.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DemoWebService.Extensions
{
    public static class ComponentsExtensions
    {
        public static IServiceCollection ConfigureComponents(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AutoReloadingResolverConfig>(configuration.GetSection("Resolver"));
            services.AddSingleton<IAutoReloadingResolver>(s =>
                new AutoReloadingResolver(s.GetService<IOptions<AutoReloadingResolverConfig>>())
            );

            return services;
        }
    }
}
