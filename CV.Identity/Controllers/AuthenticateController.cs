using CV.Identity.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using CV.Identity.Services.TokenService;
using CV.Identity.Services.UserService;
using CV.Identity.Requests.TokenRequests;
using CV.Identity.Services.ConfigurationService;
using CV.Identity.Services.ApiKeyService;
namespace CV.Identity.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IApiKeyService _apiKeyService;

        public AuthenticateController(IConfigurationService configurationService, ITokenService tokenService, IUserService userService, IApiKeyService apiKeyService)
        {
            _configurationService = configurationService;
            _tokenService = tokenService;
            _userService = userService;
            _apiKeyService = apiKeyService;
        }

        [Authorize]
        [HttpGet(ApiEndpoints.PersonalApiKeys.Base)]
        public async Task<IActionResult> GeneratePersonalApiKey()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(ModelState);
            }

            var expClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
 

            var issuer = _configurationService.GetJwtApiIssuer();
            var audience = _configurationService.GetJwtApiAudience();
            var apiKeyClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                };
            Console.WriteLine(userId);
            var apiKey = _tokenService.GenerateJwtToken(apiKeyClaims, issuer, audience, TimeSpan.FromDays(365));
            await _apiKeyService.RevokePreviousApiKey(userId, apiKey);
            await _apiKeyService.StoreApiKey(userId, apiKey, DateTime.UtcNow.AddYears(1));


            return Ok(new { apiKey });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            try
            {
                var tokenResponse = await _tokenService.RefreshAccessTokenAsync(request.RefreshToken);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid token." });
            }
        }



        [HttpPost(ApiEndpoints.OAuthProviders.Google)]
        public async Task<IActionResult> ValidateToken([FromBody] TokenModel tokenModel)
        {
            var validPayload = await GoogleJsonWebSignature.ValidateAsync(tokenModel.Token, new GoogleJsonWebSignature.ValidationSettings());

            var issuer = _configurationService.GetJwtConfigIssuer();
            var audience = _configurationService.GetJwtConfigAudience();

            var userExists = await _userService.UserExists(validPayload.Subject);
            if (!userExists)
            {
                await _userService.CreateUser(new User
                {
                    UserId = validPayload.Subject,
                    Email = validPayload.Email,
                    FirstName = validPayload.Name,
                    LastName = validPayload.FamilyName,
                    CountryId = 242, //"other" country in countries table
                    OpenForWork = true, //default
                });
            }


            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, validPayload.Subject),
            new(JwtRegisteredClaimNames.Email, validPayload.Email),
        };

            if (!string.IsNullOrEmpty(validPayload.Picture))
            {
                var profileImageClaim = new Claim("profileImage", validPayload.Picture);
                claims.Add(profileImageClaim);
            }
            var jwtToken = _tokenService.GenerateJwtToken(claims, issuer, audience, TimeSpan.FromSeconds(3));
            var refresh_Token = await _tokenService.GenerateAndStoreRefreshToken(validPayload.Subject);

            return Ok(new { accessToken = jwtToken, refreshToken = refresh_Token });
        }
    }
}
