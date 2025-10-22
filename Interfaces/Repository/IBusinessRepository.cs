using VaultIQ.Models;

namespace VaultIQ.Interfaces.Repository
{
    public interface IBusinessRepository
    {
        Task<Business> GetByEmailAsync(string email);
        Task AddBusinessAsync(Business business);
        Task UpdateBusinessAsync(Business business);
        Task<bool> SaveChangesAsync();
    }
}
