using DemoWebService.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DemoWebService.Extensions
{
    public static class MvcExtensions
    {
        public static IServiceCollection ConfigureMvc(this IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                    options.SerializerSettings.Converters.Add(new IPAddressConverter());

                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            return services;
        }
    }
}
