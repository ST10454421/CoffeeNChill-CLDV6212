using Azure.Storage.Files.Shares;
using CoffeeNChill.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChill.Api.Controllers;

/// <summary>Staff documents stored in the staff-docs Azure File Share (Admin or Staff only).</summary>
[ApiController]
[Route("api/documents")]
[Authorize(Roles = "Admin,Staff")]
public class DocumentsController : ControllerBase
{
    private readonly IConfiguration _config;
    public DocumentsController(IConfiguration config) => _config = config;

    private async Task<ShareDirectoryClient> GetRootAsync()
    {
        var conn = _config["FileShareConnection"]
            ?? throw new InvalidOperationException("FileShareConnection is not configured.");
        var share = new ShareClient(conn, "staff-docs");
        await share.CreateIfNotExistsAsync();
        return share.GetRootDirectoryClient();
    }

    /// <summary>Uploads a document to the staff-docs share.</summary>
    /// <response code="201">File stored.</response>
    /// <response code="400">No file supplied.</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
    {
        var file = dto.File;
        if (file is null || file.Length == 0) return BadRequest("A file is required.");

        var name = Path.GetFileName(file.FileName);
        var client = (await GetRootAsync()).GetFileClient(name);
        await client.CreateAsync(file.Length);
        using var stream = file.OpenReadStream();
        await client.UploadAsync(stream);
        return StatusCode(StatusCodes.Status201Created, new { fileName = name, size = file.Length });
    }

    /// <summary>Lists stored documents with size and last modified date.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        var root = await GetRootAsync();
        var files = new List<object>();
        await foreach (var item in root.GetFilesAndDirectoriesAsync())
        {
            if (item.IsDirectory) continue;
            var props = (await root.GetFileClient(item.Name).GetPropertiesAsync()).Value;
            files.Add(new { fileName = item.Name, size = props.ContentLength, lastModified = props.LastModified });
        }
        return Ok(files);
    }

    /// <summary>Downloads a document.</summary>
    [HttpGet("download/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string fileName)
    {
        var client = (await GetRootAsync()).GetFileClient(Path.GetFileName(fileName));
        if (!await client.ExistsAsync()) return NotFound();
        var stream = await client.OpenReadAsync();
        return File(stream, "application/octet-stream", fileName);
    }

    /// <summary>Deletes a document.</summary>
    [HttpDelete("{fileName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string fileName)
    {
        var client = (await GetRootAsync()).GetFileClient(Path.GetFileName(fileName));
        var deleted = await client.DeleteIfExistsAsync();
        return deleted.Value ? NoContent() : NotFound();
    }
}