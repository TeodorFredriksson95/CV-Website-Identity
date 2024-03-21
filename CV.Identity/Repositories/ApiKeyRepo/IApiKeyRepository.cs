using CV.Identity.Models;
using System.Data;

namespace CV.Identity.Repositories.ApiKeyRepo
{
    public interface IApiKeyRepository
    {
        Task StoreApiKey(string userId, string apiKey, DateTime expiresAt);
        Task RevokeApiKey(string apiKey, string replacedByApiKey = null);
        Task<ApiToken> GetByApiKeyAsync(string apiKey);
        Task <ApiToken>GetUserLastActiveApiKeyAsync(string userId);

    }
}
