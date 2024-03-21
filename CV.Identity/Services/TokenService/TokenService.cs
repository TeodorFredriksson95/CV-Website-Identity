using CV.Identity.Database;
using CV.Identity.Models;
using CV.Identity.Repositories.RefreshTokenRepo;
using CV.Identity.Responses.TokenResponses;
using CV.Identity.Services.ConfigurationService;
using CV.Identity.Services.UserService;
using Microsoft.AspNetCore.Connections;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CV.Identity.Services.TokenService
{
    public class TokenService : ITokenService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserService _userService;
        public TokenService(IConfigurationService configurationService, IRefreshTokenRepository refreshTokenRepository, IUserService userService)
        {
            _configurationService = configurationService;
            _refreshTokenRepository = refreshTokenRepository;
            _userService = userService;
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

        }

        public async Task<TokenResponse> RefreshAccessTokenAsync(string oldRefreshToken)
        {
            var oldTokenDetails = await _refreshTokenRepository.GetByTokenAsync(oldRefreshToken);
            if (oldTokenDetails == null || oldTokenDetails.IsExpired || oldTokenDetails.Revoked)
            {
                Console.WriteLine(oldTokenDetails);
                throw new Exception("Invalid refresh token.");
            }


            var userId = oldTokenDetails.UserId;

            RefreshToken newRefreshToken = oldTokenDetails;

            if ((oldTokenDetails.ExpiresAt - DateTime.UtcNow).TotalDays < 1)
            {
                newRefreshToken = GenerateRefreshToken();
                await _refreshTokenRepository.RevokeRefreshTokenAsync(oldRefreshToken, newRefreshToken.Token); // Optionally track the new token
                await _refreshTokenRepository.StoreRefreshToken(userId, newRefreshToken.Token, newRefreshToken.ExpiresAt);
            }

            var userClaims = await _userService.GetUserClaims(userId);
            var issuer = _configurationService.GetJwtConfigIssuer();
            var audience = _configurationService.GetJwtConfigAudience();
            var newAccessToken = GenerateJwtToken(userClaims.ToList(), issuer, audience, TimeSpan.FromSeconds(3));


            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task<RefreshToken> GenerateAndStoreRefreshToken(string userId)
        {

            var refreshToken = GenerateRefreshToken();
            refreshToken.UserId = userId;

            await _refreshTokenRepository.GenerateAndStoreRefreshToken(refreshToken);

            return refreshToken;
        }

        public RefreshToken GenerateRefreshToken()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            var token = Convert.ToBase64String(randomBytes);

            return new RefreshToken
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                Revoked = false
            };
        }

    }
}
