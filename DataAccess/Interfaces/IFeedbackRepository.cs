using DataAccess.DTOs.UserDTO;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace DataAccess.Interfaces
{
    public interface IFeedbackRepository
    {
        PagedResult<FeedbackDto> GetFeedbacks(string? search, string? type, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<List<Feedback>> GetFeedbackByUserId(int userId);
        void AddFeedback(Feedback feedback);
        Task<Feedback?> GetByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
