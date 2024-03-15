namespace CV.Identity
{
    public interface IConfigurationService
    {
        string GetJwtSecretKey();
        string GetJwtConfigIssuer();
        string GetJwtConfigAudience();
        string GetJwtApiIssuer();
        string GetJwtApiAudience();
    }
}
