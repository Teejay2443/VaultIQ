﻿using Microsoft.EntityFrameworkCore;
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

    }
}
