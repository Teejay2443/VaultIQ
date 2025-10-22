using VaultIQ.Models;

namespace VaultIQ.Interfaces.Repository
{
    public interface IDataRequestRepository
    {
        Task AddDataRequestAsync(DataRequest request);
        Task<DataRequest?> GetByIdAsync(Guid id);
        Task<IEnumerable<DataRequest>> GetByUserEmailAsync(string email);
        Task<IEnumerable<DataRequest>> GetByBusinessIdAsync(Guid businessId);
        Task UpdateDataRequestAsync(DataRequest request);
        Task<bool> SaveChangesAsync();
    }

}
