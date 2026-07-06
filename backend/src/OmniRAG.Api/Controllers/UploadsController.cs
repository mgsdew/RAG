using Microsoft.AspNetCore.Mvc;
using OmniRAG.Api.Models;
using OmniRAG.Application.Abstractions;
using OmniRAG.Application.Dtos;
using OmniRAG.Domain.Entities;

namespace OmniRAG.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public UploadsController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UploadRecordDto>>> GetAll(CancellationToken cancellationToken)
    {
        var records = await _uploadService.GetAllAsync(cancellationToken);
        return Ok(records);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UploadRecordDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var record = await _uploadService.GetByIdAsync(id, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadRecordDto>> Create([FromForm] UploadCreateForm form, CancellationToken cancellationToken)
    {
        if (form.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        if (!UploadRecord.IsSupportedFileName(form.File.FileName))
        {
            return BadRequest("Only CSV, XLS, and XLSX files are supported.");
        }

        var fileName = string.IsNullOrWhiteSpace(form.FileName)
            ? Path.GetFileName(form.File.FileName)
            : form.FileName.Trim();

        await using var fileStream = form.File.OpenReadStream();
        var created = await _uploadService.CreateAsync(fileStream, fileName, form.File.ContentType ?? "application/octet-stream", form.UploadedAt, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadRecordDto>> Update(Guid id, [FromForm] UploadUpdateForm form, CancellationToken cancellationToken)
    {
        if (form.File is null && string.IsNullOrWhiteSpace(form.FileName) && string.IsNullOrWhiteSpace(form.ContentType) && form.UploadedAt is null)
        {
            return BadRequest("Provide at least one value to update.");
        }

        if (form.File is not null && form.File.Length > 0 && !UploadRecord.IsSupportedFileName(form.File.FileName))
        {
            return BadRequest("Only CSV, XLS, and XLSX files are supported.");
        }

        var fileName = form.File is not null && string.IsNullOrWhiteSpace(form.FileName)
            ? Path.GetFileName(form.File.FileName)
            : form.FileName;

        var contentType = form.File is not null && string.IsNullOrWhiteSpace(form.ContentType)
            ? form.File.ContentType
            : form.ContentType;

        await using Stream? fileStream = form.File is null ? null : form.File.OpenReadStream();
        var updated = await _uploadService.UpdateAsync(id, fileStream, fileName, contentType, form.UploadedAt, cancellationToken);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _uploadService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
