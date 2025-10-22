using VaultIQ.Models;

namespace VaultIQ.Interfaces.Repository
{
    public interface IDocumentRespository
    {
        Task<IEnumerable<Document>> GetDocumentsByUserAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> GetTotalCountByUserAsync(Guid userId);
    }
}
