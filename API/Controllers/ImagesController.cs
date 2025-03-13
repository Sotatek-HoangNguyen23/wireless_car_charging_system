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
                return BadRequest(new { Error = "No file uploaded" });
            }

            try
            {
                var result = await _imageService.ReadSmallQrCodeWithCropAndZoom(file);
                return Ok(result);
            }
            catch (InvalidImageException ex)
            {
                return BadRequest(new
                {
                    Error = "Invalid image",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = "Error processing QR code",
                    Details = ex.Message
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
                    Message = "Xóa ảnh thành công",
                    Result = result.Result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = "Xóa ảnh thất bại",
                    Details = ex.Message
                });
            }
        }
    }
}
