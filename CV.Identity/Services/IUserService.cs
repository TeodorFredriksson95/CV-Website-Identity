using CV.Identity.Models;

namespace CV.Identity.Services
{
    public interface IUserService
    {
        Task<bool> UserExists(string userId);
        Task CreateUser(User user);
    }
}
