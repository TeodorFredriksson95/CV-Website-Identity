using CV.Identity.Database;
using CV.Identity.Repositories.ApiKeyRepo;
using CV.Identity.Repositories.RefreshTokenRepo;
using CV.Identity.Repositories.UsersRepo;
using CV.Identity.Services.ApiKeyService;
using CV.Identity.Services.ApiTokenService;
using CV.Identity.Services.ConfigurationService;
using CV.Identity.Services.TokenService;
using CV.Identity.Services.UserService;
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
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {


            services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ITokenService, TokenService>();
            

            services.AddScoped<IApiKeyService, ApiKeyService>(); 
            services.AddScoped<IApiKeyRepository, ApiKeyRepository>(); 

            services.AddScoped<IUserService, UserService>(); 
            services.AddScoped<IUserRepository, UserRepository>(); 

            return services; 
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            string connectionString = Environment.GetEnvironmentVariable("unidevwebcon")!;
            services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
            services.AddSingleton<DbInitializer>();
            return services;
        }
    }
}
    