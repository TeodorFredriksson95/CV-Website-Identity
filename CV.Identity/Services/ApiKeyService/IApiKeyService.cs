namespace CV.Identity.Services.ApiKeyService
{
    public interface IApiKeyService
    {
        Task StoreApiKey(string userId, string apiKey, DateTime expiresAt);
        Task RevokePreviousApiKey(string userId, string newApiKey);
        //Task GenerateAndStoreNewApiKey(string userId, string ApiKey);

        //Task RevokeApiKey(string userId, string newApiKey);

    }
}
