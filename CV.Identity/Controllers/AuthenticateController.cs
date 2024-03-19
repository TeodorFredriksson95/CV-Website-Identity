using CV.Identity.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using CV.Identity.Services;
namespace CV.Identity.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public AuthenticateController(IConfigurationService configurationService, ITokenService tokenService, IUserService userService)
        {
            _configurationService = configurationService;
            _tokenService = tokenService;
            _userService = userService;
        }

        [Authorize]
        [HttpGet(ApiEndpoints.PersonalApiKeys.Base)]
        public IActionResult GeneratePersonalApiKey()
        {
     
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(ModelState);
            }
            var issuer = _configurationService.GetJwtApiIssuer();
            var audience = _configurationService.GetJwtApiAudience();
            var apiKeyClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
            };


            var apiKey = _tokenService.GenerateJwtToken(apiKeyClaims, issuer, audience, TimeSpan.FromDays(365)); 
            return Ok(new { apiKey });
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
            var jwtToken = _tokenService.GenerateJwtToken(claims, issuer, audience, TimeSpan.FromHours(1));
            var refresh_Token = await _tokenService.GenerateAndStoreRefreshToken(validPayload.Subject);

            return Ok(new { accessToken = jwtToken, refreshToken = refresh_Token });
        }
    }
}
