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
