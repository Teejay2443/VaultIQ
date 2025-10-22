using Microsoft.EntityFrameworkCore;
using VaultIQ.Models;

namespace VaultIQ.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<DataRequest> DataRequests { get; set; }
    }
}
