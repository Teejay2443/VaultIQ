
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VaultIQ.Data;
using VaultIQ.Dtos;
using VaultIQ.Dtos.Responses;
using VaultIQ.Interfaces.Repository;
using VaultIQ.Interfaces.Services;
using VaultIQ.Models;
using VaultIQ.Repositories;
using VaultIQ.Settings;

namespace VaultIQ.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly Cloudinary _cloudinary;

        private readonly AppDbContext _context;
        private readonly IDocumentRespository _documentRepository; 

        public DocumentService(IOptions<CloudinarySettings> config, AppDbContext context, IDocumentRespository documentRepository)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _context = context;
            _documentRepository = documentRepository; 
        }

        public async Task<Document> UploadAsync(UploadDocumentDto request, Guid userId)
        {
            if (request.File == null || request.File.Length == 0)
                throw new Exception("No file was uploaded.");

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(request.File.FileName, request.File.OpenReadStream()),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                AccessMode = "public" 
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            var document = new Document
            {
                FileName = uploadResult.OriginalFilename ?? request.File.FileName,
                FileUrl = uploadResult.SecureUrl.ToString(),
                FileType = request.File.ContentType,
                PublicId = uploadResult.PublicId,
                UserId = userId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }
        public async Task<PagedDocumentsDto> GetDocumentsByUserEmailAsync(string email, int pageNumber, int pageSize)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email is required.");

            var docs = await _documentRepository.GetDocumentsByUserEmailAsync(email, pageNumber, pageSize);
            var total = await _documentRepository.GetTotalCountByUserEmailAsync(email);

            return new PagedDocumentsDto
            {
                Documents = docs.Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.FileType,
                    d.FileUrl,
                    d.UploadedAt,
                }),
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }



        public async Task<PagedDocumentsDto> GetAllDocumentsAsync(Guid userId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var docs = await _documentRepository.GetDocumentsByUserAsync(userId, pageNumber, pageSize);
            var total = await _documentRepository.GetTotalCountByUserAsync(userId);

            return new PagedDocumentsDto
            {
                Documents = docs.Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.FileType,
                    d.FileUrl,
                    d.UploadedAt,
                }),
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ResponseModel> DeleteDocumentAsync(Guid documentId)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                    return ResponseModel.Failure("Document not found.");

                // ✅ Use the stored PublicId
                if (!string.IsNullOrEmpty(document.PublicId))
                {
                    var deletionParams = new DeletionParams(document.PublicId)
                    {
                        ResourceType = ResourceType.Raw // since you used RawUploadParams
                    };

                    var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                    if (deletionResult.Result != "ok" && deletionResult.Result != "not found")
                        return ResponseModel.Failure($"Failed to delete from Cloudinary: {deletionResult.Result}");
                }

                // ✅ Delete from local DB
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return ResponseModel.Success("Document deleted successfully.");
            }
            catch (Exception ex)
            {
                return ResponseModel.Failure($"Error deleting document: {ex.Message}");
            }
        }

        public async Task<PagedDocumentsDto> SearchDocumentsAsync(
       Guid userId, string query, int pageNumber, int pageSize)
        {
            query = query.ToLower();

            var documentsQuery = _context.Documents
                .Where(d => d.UserId == userId &&
                       (d.FileName.ToLower().Contains(query) ||
                        d.FileType.ToLower().Contains(query)));

            var totalCount = await documentsQuery.CountAsync();

            var documents = await documentsQuery
                .OrderByDescending(d => d.UploadedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileType = d.FileType,
                    FileUrl = d.FileUrl,
                    UploadedAt = d.UploadedAt
                })
                .ToListAsync();

            return new PagedDocumentsDto
            {
                Documents = documents,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}

