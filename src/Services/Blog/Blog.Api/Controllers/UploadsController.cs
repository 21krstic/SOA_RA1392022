using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Controllers;

public record UploadImageResponse(string Path);

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    private readonly IWebHostEnvironment _env;

    public UploadsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [Authorize]
    [HttpPost("blog-image")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<UploadImageResponse>> UploadBlogImage(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("Empty file.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension)) return BadRequest("Unsupported image type.");

        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "blogs");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return Ok(new UploadImageResponse($"/uploads/blogs/{fileName}"));
    }
}
