namespace CV.Identity
{
    public interface IConfigurationService
    {
        string GetJwtSecretKey();
        string GetJwtIssuer();
        string GetJwtAudience();
    }
}
