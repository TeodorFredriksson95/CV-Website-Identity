using CV.Identity.Database;
using CV.Identity.Repositories;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Connections;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CV.Identity.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IDbConnectionFactory _connectionFactory;
        public TokenService(IConfigurationService configurationService, IRefreshTokenRepository refreshTokenRepository, IDbConnectionFactory dbConnectionFactory)
        {
            _configurationService = configurationService;
            _refreshTokenRepository = refreshTokenRepository;
            _connectionFactory = dbConnectionFactory;
        }
        public string GenerateJwtToken(List<Claim> claims, string issuer, string audience, TimeSpan lifespan)
        {
            var key = Encoding.UTF8.GetBytes(_configurationService.GetJwtSecretKey());

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.Add(lifespan),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }// Inside your TokenService class

        public async Task<TokenResponse> RefreshAccessTokenAsync(string refreshTokenValue)
        {

            //var refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshTokenValue);
            //if (refreshToken == null || refreshToken.IsExpired || refreshToken.Revoked)
            //{
            //    throw new Exception("Invalid refresh token.");
            //}

            //// Assuming you have a method to get claims for a user
            //var claims = await GetUserClaims(refreshToken.UserId);

            //// Generate a new access token
            //var accessToken = GenerateJwtToken(claims, _configurationService.GetJwtApiIssuer(), _configurationService.GetJwtApiAudience(), TimeSpan.FromHours(1));

            //// Optionally, generate a new refresh token and invalidate the old one
            //var newRefreshToken = Guid.NewGuid().ToString();
            //refreshToken.Token = newRefreshToken; // Update the token value to the new one
            //refreshToken.Expires = DateTime.UtcNow.AddDays(7); // Extend the expiration
            //await _refreshTokenRepository.UpdateRefreshTokenAsync(refreshToken, dbConnection);

            //return new TokenResponse
            //{
            //    AccessToken = accessToken,
            //    RefreshToken = newRefreshToken
            //};
            throw new NotImplementedException();
        }
        public async Task<string> GenerateAndStoreRefreshToken(string userId)
        {

            var refreshToken = Guid.NewGuid().ToString();
            var expiryDate = DateTime.UtcNow.AddDays(7); 

            await _refreshTokenRepository.GenerateAndStoreRefreshToken(userId, refreshToken, expiryDate);

            return refreshToken;
        }


    }
}
