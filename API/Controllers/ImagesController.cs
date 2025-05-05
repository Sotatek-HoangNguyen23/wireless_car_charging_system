using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ImageService _imageService;

        public ImagesController(ImageService imageService)
        {
            _imageService = imageService;
        }
        [HttpPost("read-qr")]
        public async Task<IActionResult> ReadQrCode(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title= "Invalid Request",
                    Detail = "Hãy upload ảnh đúng",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var result = await _imageService.ReadSmallQrCode(file);
                return Ok(new { Result = result });
            }
            catch (InvalidImageException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title= "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
        [HttpPost("read-qr/url")]
        public async Task<IActionResult> ReadQrCodeUrl([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "Hãy nhập url ảnh hợp lệ",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            try
            {
                var result = await _imageService.ReadQrCodeUrl(url);
                return Ok(new { Result = result });
            }
            catch (InvalidImageException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteImage(string publicId)
        {
            try
            {
                // Xóa ảnh từ Cloudinary
                var result = await _imageService.DeleteImageAsync(publicId);

                return Ok(new 
                {
                    Title = "Delete Image Success",
                    Message = "Xóa ảnh thành công",
                    Detail = result.Result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Delete Image Failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }
    }
}
