using CV.Identity.Models;

namespace CV.Identity.Repositories.UsersRepo
{
    public interface IUserRepository
    {
        Task<bool> UserExists(string userId);
        Task CreateUser(User user);

        Task<User> GetUser(string userId);
        Task UpdateLoginCount(string userId);

    }
}
