using CV.Identity.Models;
using CV.Identity.Repositories.UsersRepo;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CV.Identity.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task CreateUser(User user)
        {
            await _userRepository.CreateUser(user);
        }

        public async Task<IEnumerable<Claim>> GetUserClaims(string userId)
        {
            var user = await _userRepository.GetUser(userId);
            if (user == null)
            {
                throw new Exception("user not found");
            }
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };
            return claims;
        }

        public async Task UpdateLoginCount(string userId)
        {
            await _userRepository.UpdateLoginCount(userId);
        }

        public async Task<bool> UserExists(string userId)
        {
            return await _userRepository.UserExists(userId);
        }
    }
}
