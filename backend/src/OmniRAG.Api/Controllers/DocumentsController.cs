using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OmniRAG.Api.Models;
using OmniRAG.Application.Abstractions;
using OmniRAG.Application.Dtos;
using OmniRAG.Domain.Entities;

namespace OmniRAG.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/Documents")]
public class DocumentsController : ControllerBase
{
 //   private readonly IUploadService _uploadService;

    private readonly RagSqlDbContext _dbContext;
    private readonly Container _cosmosContainer;

    public DocumentsController(RagSqlDbContext dbContext, Container cosmosContainer)
    {
      //  _uploadService = uploadService;
        _dbContext = dbContext;
        _cosmosContainer = cosmosContainer;
    }

    [Authorize(Roles = "Document.Uploader")]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        // 1. Generate unique identifier for the document tracking
        var documentId = Guid.NewGuid().ToString();

        // 2. Read file content safely as raw text
        using var reader = new StreamReader(file.OpenReadStream());
        var rawTextContent = await reader.ReadToEndAsync();

        // 3. Save RAW content to NoSQL (Cosmos DB)
        var cosmosItem = new
        {
            id = documentId,       // item unique id
            documentId = documentId, // Partition Key mapping
            fileName = file.FileName,
            rawContent = rawTextContent,
            chunked = false          // RAG pipeline state tracking
        };

        await _cosmosContainer.CreateItemAsync(cosmosItem, new PartitionKey(documentId));

        // 4. Save STRUCTURED relational metadata to Azure SQL
        var metadata = new DocumentMetadata
        {
            DocumentId = documentId, 
            FileName = file.FileName,
            FileSizeInBytes = file.Length,
            UploadedAt = DateTime.UtcNow + TimeSpan.FromHours(12),
            UploadedBy = "LocalTestUser" // To be dynamically populated by OAuth2 later
        };

        _dbContext.DocumentMetadata.Add(metadata);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Successfully processed and saved to hybrid data layers.", DocumentId = documentId });
    }

    //[HttpGet]
    //public async Task<ActionResult<IReadOnlyCollection<UploadRecordDto>>> GetAll(CancellationToken cancellationToken)
    //{
    //    var records = await _uploadService.GetAllAsync(cancellationToken);
    //    return Ok(records);
    //}

    //[HttpGet("{id:guid}")]
    //public async Task<ActionResult<UploadRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
    //{
    //    var record = await _uploadService.GetByIdAsync(id, cancellationToken);
    //    return record is null ? NotFound() : Ok(record);
    //}

    //[HttpPost]
    //[Consumes("multipart/form-data")]
    //public async Task<ActionResult<UploadRecordDto>> Create([FromForm] UploadCreateForm form, CancellationToken cancellationToken)
    //{
    //    if (form.File.Length == 0)
    //    {
    //        return BadRequest("File is required.");
    //    }

    //    if (!UploadRecord.IsSupportedFileName(form.File.FileName))
    //    {
    //        return BadRequest("Only CSV, XLS, and XLSX files are supported.");
    //    }

    //    var fileName = string.IsNullOrWhiteSpace(form.FileName)
    //        ? Path.GetFileName(form.File.FileName)
    //        : form.FileName.Trim();

    //    await using var fileStream = form.File.OpenReadStream();
    //    var created = await _uploadService.CreateAsync(fileStream, fileName, form.File.ContentType ?? "application/octet-stream", form.UploadedAt, cancellationToken);

    //    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    //}

    //[HttpPut("{id:guid}")]
    //[Consumes("multipart/form-data")]
    //public async Task<ActionResult<UploadRecordDto>> Update(Guid id, [FromForm] UploadUpdateForm form, CancellationToken cancellationToken)
    //{
    //    if (form.File is null && string.IsNullOrWhiteSpace(form.FileName) && string.IsNullOrWhiteSpace(form.ContentType) && form.UploadedAt is null)
    //    {
    //        return BadRequest("Provide at least one value to update.");
    //    }

    //    if (form.File is not null && form.File.Length > 0 && !UploadRecord.IsSupportedFileName(form.File.FileName))
    //    {
    //        return BadRequest("Only CSV, XLS, and XLSX files are supported.");
    //    }

    //    var fileName = form.File is not null && string.IsNullOrWhiteSpace(form.FileName)
    //        ? Path.GetFileName(form.File.FileName)
    //        : form.FileName;

    //    var contentType = form.File is not null && string.IsNullOrWhiteSpace(form.ContentType)
    //        ? form.File.ContentType
    //        : form.ContentType;

    //    await using Stream? fileStream = form.File is null ? null : form.File.OpenReadStream();
    //    var updated = await _uploadService.UpdateAsync(id, fileStream, fileName, contentType, form.UploadedAt, cancellationToken);

    //    return updated is null ? NotFound() : Ok(updated);
    //}

    //[HttpDelete("{id:guid}")]
    //public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    //{
    //    var deleted = await _uploadService.DeleteAsync(id, cancellationToken);
    //    return deleted ? NoContent() : NotFound();
    //}
}
