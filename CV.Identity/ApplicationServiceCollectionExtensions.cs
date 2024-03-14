
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV.Identity
{
    public static class ApplicationServiceCollectionExtensions
    {

        public static void AddJWTService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
        }
        public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins(configuration["AllowedOrigins"]!.Split(","))
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
        }
    }
}
