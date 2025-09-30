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
                    return BadRequest(new { message = "请选择要上传的图片文件" });
                }
                
                // 验证文件类型
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "只支持 JPG、PNG、GIF、WEBP 格式的图片文件" });
                }
                
                // 验证文件大小 (最大5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "图片文件大小不能超过5MB" });
                }
                
                // 创建上传目录
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "menu-images");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                
                // 生成唯一文件名
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                
                // 保存文件
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // 返回文件URL
                var fileUrl = $"/uploads/menu-images/{fileName}";
                
                _logger.LogInformation($"图片上传成功: {fileUrl}");
                
                return Ok(new 
                { 
                    message = "图片上传成功",
                    imageUrl = fileUrl,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "图片上传失败");
                return StatusCode(500, new { message = "图片上传失败", error = ex.Message });
            }
        }
        
        [HttpDelete("image")]
        public ActionResult DeleteImage([FromQuery] string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { message = "文件名不能为空" });
                }
                
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "menu-images", fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"图片删除成功: {fileName}");
                    return Ok(new { message = "图片删除成功" });
                }
                else
                {
                    return NotFound(new { message = "图片文件不存在" });
                }
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "图片删除失败");
                return StatusCode(500, new { message = "图片删除失败", error = ex.Message });
            }
        }
    }
}