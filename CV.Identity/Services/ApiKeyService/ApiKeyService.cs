using CV.Identity.Repositories.ApiKeyRepo;
using CV.Identity.Services.ApiKeyService;

namespace CV.Identity.Services.ApiTokenService
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        public ApiKeyService(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository;
        }
        //public async Task RevokeApiKey(string userId, string newApiKey)
        //{
        //    var currentApiObject = await _apiKeyRepository.GetByApiKeyAsync(userId);
        //    var currentApiKey = currentApiObject.ApiKey;

        //    await _apiKeyRepository.RevokeApiKey(currentApiKey, replacedByApiKey);
        //}

        //public async Task GenerateAndStoreNewApiKey(string userId, string apiKey)
        //{
        //    await RevokePreviousApiKey(userId, apiKey);
        //    await StoreApiKey(userId, apiKey, DateTime.UtcNow.AddYears(1));
        //}

        public async Task StoreApiKey(string userId, string apiKey, DateTime expiresAt)
        {
            await _apiKeyRepository.StoreApiKey(userId, apiKey, expiresAt);
        }

        public async Task RevokePreviousApiKey(string userId, string newApiKey)
        {
            var lastActiveApiKey = await _apiKeyRepository.GetUserLastActiveApiKeyAsync(userId);

            if (lastActiveApiKey != null)
            {
                await _apiKeyRepository.RevokeApiKey(lastActiveApiKey.ApiKey, newApiKey);
            }
            

        }
    }
}
