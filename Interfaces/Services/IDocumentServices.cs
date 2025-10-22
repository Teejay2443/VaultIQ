using VaultIQ.Dtos;
using VaultIQ.Dtos.Responses;
using VaultIQ.Models;

namespace VaultIQ.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<Document> UploadAsync(UploadDocumentDto request, Guid userId);
        Task<PagedDocumentsDto> GetAllDocumentsAsync(Guid userId, int pageNumber, int pageSize);
        Task<ResponseModel> DeleteDocumentAsync(Guid documentId);
    }
}
