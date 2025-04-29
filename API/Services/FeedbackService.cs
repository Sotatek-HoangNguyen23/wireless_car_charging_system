using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using System.Security;

namespace API.Services
{
    public class FeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public PagedResult<FeedbackDto> GetFeedbacks(string? search, string? type, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            return _feedbackRepository.GetFeedbacks(search, type, status, startDate, endDate, page, pageSize);
        }

        public void AddFeedback(AddFeedbackDto dto)
        {
            var feedback = new Feedback
            {
                UserId = dto.UserId,
                Message = dto.Message,
                Type = dto.Type,
                CarId = dto.Type == "Car" ? dto.CarId : null,
                StationId = dto.Type == "Station" ? dto.StationId : null,
                PointId = dto.Type == "Station" ? dto.PointId : null,
                CreatedAt = DateTime.Now,
                Status = "Pending"
            };

            _feedbackRepository.AddFeedback(feedback);
        }

        public async Task<bool> ResolveFeedback(int id, string status, string response)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);
            if (feedback == null || feedback.Status == "Processed")
                return false;

            feedback.Status = status;
            feedback.Response = response;
            await _feedbackRepository.SaveChangesAsync();
            return true;
        }
    }
}
