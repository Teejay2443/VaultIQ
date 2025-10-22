using Microsoft.EntityFrameworkCore;
using VaultIQ.Data;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Models;

namespace VaultIQ.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
        }

    }
}
