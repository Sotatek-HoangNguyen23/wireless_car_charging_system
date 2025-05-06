using DataAccess.DTOs;
using DataAccess.DTOs.ChargingStation;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDocumentReviewRepository
    {
        PagedResult<DocumentReviewDto>? GetAllDocumentReview(string type, string? status, int page, int pageSize);
        Task<bool> UpdateReviewInfoAsync(UpdateDocumentReviewDto dto, int currentUserId);
        Task<DocumentReview> GetDocumentReviewById(int id);

    }
}
