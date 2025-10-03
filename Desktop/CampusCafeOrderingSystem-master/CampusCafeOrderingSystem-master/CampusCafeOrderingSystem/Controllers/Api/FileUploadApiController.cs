using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Vendor")]
    public class FileUploadApiController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadApiController> _logger;
        
        public FileUploadApiController(IWebHostEnvironment environment, ILogger<FileUploadApiController> logger)
        {
            _environment = environment;
            _logger = logger;
        }
        
        [HttpPost("image")]
        public async Task<ActionResult<object>> UploadImage([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Please select an image file to upload" });
                }
                
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "Only JPG, PNG, GIF, WEBP format image files are supported" });
                }
                
                // Validate file size (maximum 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Image file size cannot exceed 5MB" });
                }
                
                // Create upload directory
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "menu-images");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                
                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                
                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Return file URL
                var fileUrl = $"/uploads/menu-images/{fileName}";
                
                _logger.LogInformation($"Image upload successful: {fileUrl}");
                
                return Ok(new 
                { 
                    message = "Image upload successful",
                    imageUrl = fileUrl,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image upload failed");
                return StatusCode(500, new { message = "Image upload failed", error = ex.Message });
            }
        }
        
        [HttpDelete("image")]
        public ActionResult DeleteImage([FromQuery] string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { message = "Filename cannot be empty" });
                }
                
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "menu-images", fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"Image deleted successfully: {fileName}");
                    return Ok(new { message = "Image deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Image file does not exist" });
                }
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Image deletion failed");
                return StatusCode(500, new { message = "Image deletion failed", error = ex.Message });
            }
        }
    }
}