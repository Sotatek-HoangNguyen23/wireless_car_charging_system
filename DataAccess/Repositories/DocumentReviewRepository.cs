using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class DocumentReviewRepository : IDocumentReviewRepository
    {
        private readonly WccsContext _context;

        public DocumentReviewRepository(WccsContext context)
        {
            _context = context;
        }

        public PagedResult<DocumentReviewDto> GetAllDocumentReview(string type, string? status, int page, int pageSize)
        {
            var document = _context.DocumentReviews
                .Include(d => d.ReviewedByNavigation)    
                .Where(d => d.ReviewType.Equals(type))
                .OrderBy(d => d.Status != "PENDING")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                document = document.Where(d => d.Status.Equals(status));
            }

            var documentList = document.Select(d => new DocumentReviewDto
            {
                Id = d.ReviewId,
                Type = d.ReviewType,
                User = d.User.Email,
                Status = d.Status,
                Comments = d.Comments,
                CreateAt = d.CreateAt,
                ReviewedBy = d.ReviewedByNavigation != null ? d.ReviewedByNavigation.Fullname : "Chưa xác định",
                ReviewedAt = d.ReviewedAt.HasValue ? d.ReviewedAt.Value.ToString("dd/MM/yyyy") : "Chưa xác định"
            }).ToList();

            int totalRecords = document.Count();

            // Phân trang (chỉ lấy dữ liệu của trang hiện tại)
            var data = documentList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<DocumentReviewDto>(data, totalRecords, pageSize);
        }

        public async Task<DocumentReview?> GetDocumentReviewById(int id)
        {
            return await _context.DocumentReviews
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(d => d.ReviewId == id);
        }

        public async Task<bool> UpdateReviewInfoAsync(UpdateDocumentReviewDto dto, int currentUserId)
        {
            var doc = await _context.DocumentReviews.FindAsync(dto.Id);
            if (doc == null) return false;

            doc.Status = dto.Status;
            doc.Comments = dto.Comments;
            doc.ReviewedBy = currentUserId;
            doc.ReviewedAt = DateTime.UtcNow;
            doc.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
