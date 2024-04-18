using CV.Identity.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CV.Identity.Services.ConfigurationService
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        public string GetJwtApiAudience()
        {
            //return _configuration["ApiAccess:Audience"]!;
            return Environment.GetEnvironmentVariable("IDENTITY_APIACCESS_AUDIENCE", EnvironmentVariableTarget.Process);
        }

        public string GetJwtApiIssuer()
        {
            //return _configuration["ApiAccess:Issuer"]!;
            return Environment.GetEnvironmentVariable("IDENTITY_APIACCESS_ISSUER", EnvironmentVariableTarget.Process);
        }

        public string GetJwtConfigAudience() => Environment.GetEnvironmentVariable("IDENTITY_JWTCONFIG_AUDIENCE", EnvironmentVariableTarget.Process);

        public string GetJwtConfigIssuer()
        {
            return Environment.GetEnvironmentVariable("IDENTITY_JWTCONFIG_ISSUER", EnvironmentVariableTarget.Process);
        }

        public string GetJwtSecretKey()
        {
            return Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET", EnvironmentVariableTarget.Process);
        }

    }
}
