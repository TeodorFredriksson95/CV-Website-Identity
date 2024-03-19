using CV.Identity.Models;

namespace CV.Identity.Repositories
{
    public interface IUserRepository
    {
       Task<bool> UserExists(string userId);
       Task CreateUser(User user);

    }
}
