using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultIQ.Dtos;
using System.Security.Claims;
using Org.BouncyCastle.Bcpg;
using VaultIQ.Interfaces.Services;

namespace VaultIQ.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }



        [HttpPost("upload")]

        public async Task<IActionResult> Upload([FromForm] UploadDocumentDto request)
        {

            var userId = GetUserId();
            var document = await _documentService.UploadAsync(request, userId);

            return Ok(new
            {
                message = "File uploaded successfully",
                document
            });
        }

        [HttpGet("all")]

        public async Task<IActionResult> GetAllDocuments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();

            var response = await _documentService.GetAllDocumentsAsync(userId, pageNumber, pageSize);
            return Ok(response);
        }


        [HttpPost("upload-by-id")]
        public async Task<IActionResult> UploadById([FromForm] UploadDocumentDto request, [FromQuery] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest(new { message = "User ID is required." });

            var document = await _documentService.UploadAsync(request, userId);

            if (document == null)
                return BadRequest(new { message = "Document upload failed." });

            return Ok(new
            {
                message = "File uploaded successfully (using provided User ID).",
                document
            });
        }

        [HttpGet("get-by-user-id")]
        public async Task<IActionResult> GetDocumentsByUserId([FromQuery] Guid userId,[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
        {
            if (userId == Guid.Empty)
                return BadRequest(new { message = "User ID is required." });

            var response = await _documentService.GetAllDocumentsAsync(userId, pageNumber, pageSize);

            if (response == null)
                return NotFound(new { message = "No documents found for this user." });

            return Ok(new
            {
                message = "Documents retrieved successfully.",
                data = response
            });
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocument(Guid documentId)
        {
            if (documentId == Guid.Empty)
                return BadRequest(new { message = "Invalid document ID." });

            var result = await _documentService.DeleteDocumentAsync(documentId);

            if (!result.IsSuccessful)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

    }
}


