using VaultIQ.Models;

namespace VaultIQ.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> SaveChangesAsync();
    }
}
