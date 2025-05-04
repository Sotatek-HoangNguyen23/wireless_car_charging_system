using API.Services;
using DataAccess.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentReviewController : ControllerBase
    {
        private readonly DocumentReviewService _service;

        public DocumentReviewController(DocumentReviewService service)
        {
            _service = service;
        }

        [HttpGet("by-type")]
        public async Task<IActionResult> GetAllDocumentReview([FromQuery] string type, [FromQuery] string? status, int page = 1, int pageSize = 5)
        {
            var result = _service.GetAllDocumentReview(type, status, page, pageSize);
            return Ok(result);
        }

        [HttpPut("update-review")]
        public async Task<IActionResult> UpdateReviewInfo([FromBody] UpdateDocumentReviewDto dto)
        {
            var success = await _service.UpdateReviewInfoAsync(dto);
            if (!success) return NotFound("DocumentReview not found.");
            return Ok("Updated successfully.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var detail = await _service.GetDocumentDetail(id);
            if (detail == null)
                return NotFound();

            return Ok(detail);
        }

    }
}
