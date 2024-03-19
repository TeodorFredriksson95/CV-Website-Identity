using CV.Identity.Models;
using Google.Apis.Auth.OAuth2.Responses;
using System.Security.Claims;

namespace CV.Identity.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(List<Claim> claims, string issuer, string audience, TimeSpan lifespan);
        Task<TokenResponse> RefreshAccessTokenAsync(string refreshTokenValue);
        Task<string> GenerateAndStoreRefreshToken(string userId);


    }
}