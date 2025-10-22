using Microsoft.EntityFrameworkCore;
using VaultIQ.Data;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Models;

namespace VaultIQ.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly AppDbContext _context;

        public BusinessRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Business?> GetByEmailAsync(string email)
        {
            return await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessEmail == email);
        }

        public async Task AddBusinessAsync(Business business)
        {
            await _context.Businesses.AddAsync(business);
        }

        public async Task UpdateBusinessAsync(Business business)
        {
            _context.Businesses.Update(business);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
