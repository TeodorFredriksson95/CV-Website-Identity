using CV.Identity.Models;
using System.Security.Claims;

namespace CV.Identity.Services.UserService
{
    public interface IUserService
    {
        Task<bool> UserExists(string userId);
        Task CreateUser(User user);
        Task<IEnumerable<Claim>> GetUserClaims(string userId);

    }
}
