using Microsoft.EntityFrameworkCore;
using VaultIQ.Data;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Models;

namespace VaultIQ.Repositories
{
    public class DataRequestRepository : IDataRequestRepository
    {
        private readonly AppDbContext _context;

        public DataRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddDataRequestAsync(DataRequest request)
        {
            await _context.DataRequests.AddAsync(request);
        }

        public async Task<DataRequest?> GetByIdAsync(Guid id)
        {
            return await _context.DataRequests.Include(d => d.Business)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<DataRequest>> GetByUserEmailAsync(string email)
        {
            return await _context.DataRequests
                .Where(d => d.UserEmail == email)
                .Include(d => d.Business)
                .ToListAsync();
        }

        public async Task<IEnumerable<DataRequest>> GetByBusinessIdAsync(Guid businessId)
        {
            return await _context.DataRequests
                .Where(d => d.BusinessId == businessId)
                .Include(d => d.Business)
                .ToListAsync();
        }

        public async Task UpdateDataRequestAsync(DataRequest request)
        {
            _context.DataRequests.Update(request);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
