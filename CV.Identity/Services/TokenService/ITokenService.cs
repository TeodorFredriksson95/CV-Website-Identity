using CV.Identity.Models;
using CV.Identity.Responses.TokenResponses;
using System.Security.Claims;

namespace CV.Identity.Services.TokenService
{
    public interface ITokenService
    {
        Task<RefreshToken> GenerateAndStoreRefreshToken(string userId);
        string GenerateJwtToken(List<Claim> claims, string issuer, string audience, TimeSpan lifespan);
        Task<TokenResponse> RefreshAccessTokenAsync(string oldRefreshToken);
        RefreshToken GenerateRefreshToken();
    }
}