using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly WccsContext _context;
        public FeedbackRepository(WccsContext context)
        {
            _context = context;
        }

        public PagedResult<FeedbackDto> GetFeedbacks(string? search, string? type, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            var query = _context.Feedbacks
                .Where(f =>
                    (string.IsNullOrEmpty(search) || f.User.Email.Contains(search) || f.Message.Contains(search)) &&
                    (string.IsNullOrEmpty(type) || f.Type.Contains(type)) &&
                    (string.IsNullOrEmpty(status) || f.Status.Contains(status)) &&
                    (!startDate.HasValue || f.CreatedAt >= startDate) &&
                    (!endDate.HasValue || f.CreatedAt <= endDate)
                )
                .Select(f => new FeedbackDto
                {
                    Id = f.FeedbackId,
                    User = f.User.Email,
                    Type = f.Type,
                    Car = f.Car.LicensePlate,
                    Station = f.Station.StationName,
                    Point = f.Point.ChargingPointName,
                    Message = f.Message,
                    Status = f.Status,
                    Date = f.CreatedAt,
                    Response = f.Response
                });

            int totalCount = query.Count(); ;
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<FeedbackDto>(data, totalCount, pageSize);
        }

        public void AddFeedback(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedbacks.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
