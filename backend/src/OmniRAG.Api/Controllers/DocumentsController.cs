using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniRAG.Application.Abstractions;

namespace OmniRAG.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/Documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
                _documentService = documentService;
        }

    [Authorize(Roles = "Document.Uploader")]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var identityClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                            ?? User.FindFirst("oid");

        var uploadedBy = identityClaim?.Value ?? "UnknownApp";

        await using var stream = file.OpenReadStream();
        var documentId = await _documentService.UploadDocumentAsync(stream, file.FileName, uploadedBy, CancellationToken.None);

        return Ok(new { Message = "Successfully processed and saved to hybrid data layers.", DocumentId = documentId });
    }
}
