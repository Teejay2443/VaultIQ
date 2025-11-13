using Microsoft.EntityFrameworkCore;
using VaultIQ.Data;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Models;

namespace VaultIQ.Repositories
{
    public class DocumentRepository : IDocumentRespository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetDocumentsByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountByUserAsync(Guid userId)
        {
            return await _context.Documents.CountAsync(d => d.UserId == userId);
        }
        public async Task<Document?> GetFileByUserAndNameAsync(Guid userId, string fileName)
        {
            return await _context.Documents
    .FirstOrDefaultAsync(f => f.UserId == userId && f.FileName == fileName);
        }
        public async Task<IEnumerable<Document>> GetDocumentsByUserEmailAsync(string email, int pageNumber, int pageSize)
        {
            return await _context.Documents
                .Where(d => d.User.Email == email)
                .OrderByDescending(d => d.UploadedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountByUserEmailAsync(string email)
        {
            return await _context.Documents
                .CountAsync(d => d.User.Email == email);
        }

    }
}
