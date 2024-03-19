using CV.Identity.Models;
using System.Security.Claims;

namespace CV.Identity.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(List<Claim> claims, string issuer, string audience, TimeSpan lifespan);
        Task<TokenResponse> RefreshAccessTokenAsync(string oldRefreshToken);
        Task<RefreshToken> GenerateAndStoreRefreshToken(string userId);
        RefreshToken GenerateRefreshToken();
    }
}