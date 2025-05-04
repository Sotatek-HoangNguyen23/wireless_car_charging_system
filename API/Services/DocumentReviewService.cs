using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Repositories.StationRepo;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace API.Services
{
    public class DocumentReviewService
    {
        private readonly IDocumentReviewRepository _repository;
        private readonly ICccdRepository _cccdRepo;
        private readonly IDriverLicenseRepository _driverLicenseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMyCars _carRepo;

        public DocumentReviewService(IDocumentReviewRepository repository, ICccdRepository cccdRepo, IDriverLicenseRepository driverLicenseRepo, IUserRepository userRepository, IMyCars carRepo)
        {
            _repository = repository;
            _cccdRepo = cccdRepo;
            _driverLicenseRepo = driverLicenseRepo;
            _userRepo = userRepository;
            _carRepo = carRepo;
        }

        public PagedResult<DocumentReviewDto> GetAllDocumentReview(string type, string? status, int page, int pageSize)
        {
            return _repository.GetAllDocumentReview(type, status, page, pageSize);
            
        }

        public async Task<bool> UpdateReviewInfoAsync(UpdateDocumentReviewDto dto)
        {
            await _repository.UpdateReviewInfoAsync(dto);

            int reviewID = dto.Id;
            var document = await _repository.GetDocumentReviewById(reviewID);

            switch (dto.Type)
            {
                case "CCCD":
                    await _userRepo.ChangeUserStatusAsync(document.UserId, "Active");
                    break;
                case "DRIVER_LICENSE":
                    await _driverLicenseRepo.ChangeLicenseStatusAsync(document.DriverLicenseId, "Active");
                    break;
                case "CAR_LICENSE":
                    await _carRepo.ChangeCarStatusAsync(document.CarId, "Active");
                    break;
            }

            return true;
        }

        public async Task<DocumentDetailDto?> GetDocumentDetail(int? documentId)
        {
            if (!documentId.HasValue)
                return null;

            var document = await _repository.GetDocumentReviewById(documentId.Value);
            if (document == null)
                return null;

            if (document.CccdId.HasValue)
            {
                var cccd = await _cccdRepo.GetCccdById(document.CccdId.Value);
                if (cccd == null)
                    return null;

                return new DocumentDetailDto
                {
                    Type = "CCCD",
                    DocumentId = cccd.CccdId,
                    Code = cccd.Code,
                    Comments = document.Comments,
                    ImageFront = cccd.ImgFront,
                    ImageBack = cccd.ImgBack
                };
            }

            if (document.DriverLicenseId.HasValue)
            {
                var license = await _driverLicenseRepo.GetLicensesById(document.DriverLicenseId.Value);
                if (license == null)
                    return null;

                return new DocumentDetailDto
                {
                    Type = "Driver_License",
                    DocumentId = license.DriverLicenseId,
                    Code = license.Code,
                    Comments = document.Comments,
                    ImageFront = license.ImgFront,
                    ImageBack = license.ImgBack
                };
            }

            return null;
        }
    }
}
