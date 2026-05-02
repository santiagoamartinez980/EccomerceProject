using APIEccomerce.Models;

namespace APIEccomerce.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmail(string email);

        Task<User?> GetByEmail(string email);

        Task<User> Create(User user);
    }
}
