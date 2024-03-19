using CV.Identity.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CV.Identity
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
            return _configuration["ApiAccess:Audience"]!;
        }

        public string GetJwtApiIssuer()
        {
            return _configuration["ApiAccess:Issuer"]!;
        }

        public string GetJwtConfigAudience() => _configuration["JwtConfig:Authentication:Audience"]!;

        public string GetJwtConfigIssuer()
        {
            return _configuration["JwtConfig:Authentication:Issuer"]!;
        }

        public string GetJwtSecretKey()
        {
            return _configuration.GetValue<string>("JWT_TOKEN_SECRET")!;
        }

    }
}
