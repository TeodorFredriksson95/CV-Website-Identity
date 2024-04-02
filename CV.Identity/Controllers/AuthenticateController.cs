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
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Web;
using static System.Net.WebRequestMethods;
using static CV.Identity.Controllers.AuthenticateController;
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

    




        private async Task<string> GitHubExchangeCodeForAccessToken(string code)
        {
            var client = new HttpClient();
            var gitHubSecret = Environment.GetEnvironmentVariable("GitHub_Secret");

            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"client_id", "9ddff8bc7ebe96a759b3"},
                        {"client_secret", "c437e0f42608687d41ce3658c02dffc7e37f06c8"},
                        {"code", code},
                    });

            var response = await client.PostAsync("https://github.com/login/oauth/access_token", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parsedContent = HttpUtility.ParseQueryString(content);
                var accessToken = parsedContent["access_token"];
                Console.WriteLine(accessToken);
                return accessToken;
            }

            return null;
        }

        private async Task<GitHubUser> GetGitHubUserInfo(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

            var userResponse = await client.GetAsync("https://api.github.com/user");
            var emailResponse = await client.GetAsync("https://api.github.com/user/emails");

            if (userResponse.IsSuccessStatusCode && emailResponse.IsSuccessStatusCode)
            {
                var userContent = await userResponse.Content.ReadAsStringAsync();
                var emailContent = await emailResponse.Content.ReadAsStringAsync();

                var userInfo = JsonSerializer.Deserialize<GitHubUser>(userContent);
                var emailList = JsonSerializer.Deserialize<List<GithubEmail>>(emailContent);

                var primaryEmail = emailList.FirstOrDefault(email => email.Primary && email.Verified)?.Email;
                if (primaryEmail != null)
                {
                    userInfo.Email = primaryEmail; 
                }

                return userInfo; 
            }
            var errorContent = await userResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to exchange code for access token: {errorContent}");

            return null;
        }

        [HttpPost]
        [Route("/api/auth/github")]
        public async Task<IActionResult> HandleGitHubCallback([FromBody] GithubTokenModel code)
        {

            var accessToken = await GitHubExchangeCodeForAccessToken(code.Code);
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Failed to retrieve access token.");
            }

            var githubUser = await GetGitHubUserInfo(accessToken);
            if (githubUser == null)
            {
                return BadRequest("Failed to retrieve user information.");
            }

            var nameParts = githubUser.Name.Split(' ');

            var issuer = _configurationService.GetJwtConfigIssuer();
            var audience = _configurationService.GetJwtConfigAudience();

            var gitHubUserId = githubUser.Id.ToString();

            var userExists = await _userService.UserExists(gitHubUserId);
            if (!userExists)
            {
                await _userService.CreateUser(new User
                {
                    UserId = gitHubUserId,
                    Email = githubUser.Email,
                    FirstName = nameParts[0],
                    LastName = nameParts[1],
                    CountryId = 242, 
                });
            }


            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, gitHubUserId),
            new(JwtRegisteredClaimNames.Email, githubUser.Email),
        };

            if (!string.IsNullOrEmpty(githubUser.AvatarUrl))
            {
                var profileImageClaim = new Claim("profileImage", githubUser.AvatarUrl);
                claims.Add(profileImageClaim);
            }
            var jwtToken = _tokenService.GenerateJwtToken(claims, issuer, audience, TimeSpan.FromMinutes(60));
            var refresh_Token = await _tokenService.GenerateAndStoreRefreshToken(gitHubUserId);
            
            await _userService.UpdateLoginCount(gitHubUserId);

            return Ok(new { accessToken = jwtToken, refreshToken = refresh_Token });
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


        [HttpPost(ApiEndpoints.OAuthProviders.Linkedin)]
        public async Task<IActionResult> LinkedInCallback([FromBody] LinkedinCode code)
        {
            Console.WriteLine("second callback", code);
            var redirectUri = "https://unidevweb.com/login";


            var accessToken = await ExchangeCodeForAccessToken(code.Code, redirectUri);
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Failed to retrieve access token.");
            }

            // Step 2: Retrieve user information from LinkedIn
            var linkedInUser = await GetLinkedInUserInfo(accessToken);
            if (linkedInUser == null)
            {
                return BadRequest("Failed to retrieve user information.");
            }

            var issuer = _configurationService.GetJwtConfigIssuer();
            var audience = _configurationService.GetJwtConfigAudience();

            var userExists = await _userService.UserExists(linkedInUser.Sub);
            if (!userExists)
            {
                await _userService.CreateUser(new User
                {
                    UserId = linkedInUser.Sub,
                    Email = linkedInUser.Email,
                    FirstName = linkedInUser.Firstname,
                    LastName = linkedInUser.LastName,
                    CountryId = 242, 
                });
            }


            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, linkedInUser.Sub),
            new(JwtRegisteredClaimNames.Email, linkedInUser.Email),
        };

            if (!string.IsNullOrEmpty(linkedInUser.Picture))
            {
                var profileImageClaim = new Claim("profileImage", linkedInUser.Picture);
                claims.Add(profileImageClaim);
            }
            var jwtToken = _tokenService.GenerateJwtToken(claims, issuer, audience, TimeSpan.FromMinutes(60));
            var refresh_Token = await _tokenService.GenerateAndStoreRefreshToken(linkedInUser.Sub);

            await _userService.UpdateLoginCount(linkedInUser.Sub);

            return Ok(new { accessToken = jwtToken, refreshToken = refresh_Token });
        }

        [HttpGet]
        [Route("api/auth/linkedin/initiate")]
        public IActionResult InitiateLinkedInLogin(string state)
        {
            var clientId = "773qu5fldzxrvs";


            var linkedInUrl = $"https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id={clientId}&state={state}&redirect_uri=https://unidevweb.com/login&scope=email%20openid%20w_member_social%20profile";

            return Redirect(linkedInUrl);
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
                    FirstName = validPayload.GivenName,
                    LastName = validPayload.FamilyName,
                    CountryId = 242, //"other" country in countries table
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
            var jwtToken = _tokenService.GenerateJwtToken(claims, issuer, audience, TimeSpan.FromMinutes(60));
            var refresh_Token = await _tokenService.GenerateAndStoreRefreshToken(validPayload.Subject);

            await _userService.UpdateLoginCount(validPayload.Subject);

            return Ok(new { accessToken = jwtToken, refreshToken = refresh_Token });
        }
        public class LinkedInTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
        private async Task<string> ExchangeCodeForAccessToken(string code, string redirectUri)
        {
            var linkedinSecret = Environment.GetEnvironmentVariable("Linkedin_Secret");
            var client = new HttpClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Content-Type", "application/x-www-form-urlencoded"),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", "773qu5fldzxrvs"),
                new KeyValuePair<string, string>("client_secret", linkedinSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
            });
            Console.WriteLine($"{redirectUri}");

            var response = await client.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<LinkedInTokenResponse>(content);
                Console.WriteLine(tokenResponse?.AccessToken);
                return tokenResponse?.AccessToken;
            }

            return null;
        }
        private async Task<LinkedInUser> GetLinkedInUserInfo(string accessToken)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            Console.WriteLine(accessToken);

            var client = new HttpClient();
            var response = await client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<LinkedInUser>(content);
                return userInfo;
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to exchange code for access token: {errorContent}");

            return null;
        }

    
 
    }
}
