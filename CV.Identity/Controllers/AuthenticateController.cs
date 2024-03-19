using CV.Identity.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
namespace CV.Identity.Controllers
{
    public class AuthenticateController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

        public AuthenticateController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [Authorize]
        [HttpGet(ApiEndpoints.PersonalApiKeys.Base)]
        public IActionResult GeneratePersonalApiKey()
        {
            Console.WriteLine(User.Claims);
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(ModelState);
            }

            var apiKeyClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
            };

            var apiKey = GenerateJwtToken(apiKeyClaims); 
            return Ok(new { apiKey });
        }

        private string GenerateJwtToken(List<Claim> claims)
        {
            var key = Encoding.UTF8.GetBytes(_configurationService.GetJwtSecretKey());
            var issuer = _configurationService.GetJwtApiIssuer();
            var audience = _configurationService.GetJwtApiAudience();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost(ApiEndpoints.OAuthProviders.Google)]
        public async Task<IActionResult> ValidateToken([FromBody] TokenModel tokenModel)
        {
            var validPayload = await GoogleJsonWebSignature.ValidateAsync(tokenModel.Token, new GoogleJsonWebSignature.ValidationSettings());



            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configurationService.GetJwtSecretKey());
            var issuer = _configurationService.GetJwtConfigIssuer();
            var audience = _configurationService.GetJwtConfigAudience();


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

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifetime),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwt = tokenHandler.WriteToken(token);
            return Ok(new { token = jwt });
        }
    }
}
