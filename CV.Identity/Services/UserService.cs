using CV.Identity.Models;
using CV.Identity.Repositories;

namespace CV.Identity.Services
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

        public async Task<bool> UserExists(string userId)
        {
            return await _userRepository.UserExists(userId);
        }
    }
}
