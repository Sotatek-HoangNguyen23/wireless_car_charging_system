using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet.Core;
using DataAccess.DTOs.Auth;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace API.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICccdRepository _cccdRepository;
        private readonly ImageService _imageService;
        public UserService(IUserRepository userRepository, ICccdRepository cccdRepository, ImageService imageService)
        {
            _userRepository = userRepository;
            _cccdRepository = cccdRepository;
            _imageService = imageService;
        }
        public async Task RegisterAsync(RegisterRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Request cannot be null");
            }
            ImageUploadResult? frontUploadResult = null;
            ImageUploadResult? backUploadResult = null;
            try
            {
                _imageService.ValidateImage(request.CCCDFrontImage);
                _imageService.ValidateImage(request.CCCDBackImage);
                var frontUploadTask = _imageService.UploadImagetAsync(request.CCCDFrontImage);
                var backUploadTask = _imageService.UploadImagetAsync(request.CCCDBackImage);
                await Task.WhenAll(frontUploadTask, backUploadTask);

                frontUploadResult = await frontUploadTask;
                backUploadResult = await backUploadTask;

                var existingUser = await _userRepository.GetUserByEmail(request.Email);
                if (existingUser != null)
                {
                    throw new ArgumentException("Email already exists");
                }
                var password = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);
                var user = new User();
                user.Email = request.Email;
                user.PasswordHash = password;
                user.Fullname = request.Fullname;
                user.PhoneNumber = request.PhoneNumber;
                user.Dob = request.Dob;
                user.RoleId = 1;
                user.Gender = request.Gender;
                user.Address = request.Address;
                user.Status = "Active";
                user.CreateAt = DateTime.UtcNow;
                user.UpdateAt = DateTime.UtcNow;
                using var transaction = await _userRepository.BeginTransactionAsync();

                try
                {
                    await _userRepository.SaveUser(user);

                    var cccd = new Cccd();
                    cccd.UserId = user.UserId;
                    cccd.Code = request.CccdCode;
                    cccd.ImgFront = frontUploadResult.Url.ToString();
                    cccd.ImgFrontPubblicId = frontUploadResult.PublicId;
                    cccd.ImgBack = backUploadResult.Url.ToString();
                    cccd.ImgBackPubblicId = backUploadResult.PublicId;
                    cccd.CreateAt = DateTime.UtcNow;
                    cccd.UpdateAt = DateTime.UtcNow;

                    await _cccdRepository.SaveCccd(cccd);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Register failed", ex);
                }
            }
            catch (Exception ex)
            {
                var deleteTasks = new List<Task>();
                if (frontUploadResult != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(frontUploadResult.PublicId));
                if (backUploadResult != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(backUploadResult.PublicId));

                await Task.WhenAll(deleteTasks);
                throw new Exception("Register failed", ex);
            }

        }
        public async Task<UserDto> GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty");
            }
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var UserDto = new UserDto();
            UserDto.UserId = user.UserId;
            UserDto.Email = user.Email;
            UserDto.Fullname = user.Fullname ?? "Unknown";
            UserDto.Role = new RoleDto();
            UserDto.Role.RoleId = user.RoleId;
            UserDto.Role.Name = user.Role?.RoleName ?? "Unknown";

            return UserDto;
        }


    }
}
