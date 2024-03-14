namespace CV.Identity
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetJwtAudience() => _configuration["Jwt:Audience"]!;

        public string GetJwtIssuer()
        {
            return _configuration["Jwt:Issuer"]!;
        }

        public string GetJwtSecretKey()
        {
            return _configuration.GetValue<string>("JWT_TOKEN_SECRET")!;
        }

    }
}
