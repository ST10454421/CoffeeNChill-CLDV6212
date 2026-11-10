namespace CoffeeNChill.Api.Dtos;

/// <summary>Multipart form body for uploading a staff document.</summary>
public class UploadDocumentDto
{
    /// <summary>The file to store in the staff-docs share.</summary>
    public IFormFile File { get; set; } = default!;
}