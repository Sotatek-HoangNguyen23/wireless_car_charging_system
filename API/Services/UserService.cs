using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet.Core;
using DataAccess.DTOs;
using DataAccess.DTOs.Auth;
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

        public async Task<ProfileDTO?> GetProfileByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }

            var profile = await _userRepository.GetProfileByUserId(userId);
            if (profile == null)
            {
                throw new Exception("User profile not found.");
            }

            return profile;
        }

        public async Task UpdateUserProfileAsync(int userId, RequestProfile request)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Update user properties
            user.Fullname = request.Fullname ?? user.Fullname;
            user.Email = request.Email ?? user.Email;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.Dob = request.Dob ?? user.Dob;
            user.Gender = request.Gender ?? user.Gender;
            user.Address = request.Address ?? user.Address;
            user.UpdateAt = DateTime.UtcNow;

            await _userRepository.UpdateUser(user);
        }

        public async Task ChangePasswordAsync(ChangePassDTO passDTO)
        {
            if (string.IsNullOrWhiteSpace(passDTO.Password) ||
                string.IsNullOrWhiteSpace(passDTO.NewPassword) ||
                string.IsNullOrWhiteSpace(passDTO.ConfirmNewPassword))
            {
                throw new ArgumentException("Passwords cannot be empty.");
            }

            if (passDTO.NewPassword != passDTO.ConfirmNewPassword)
            {
                throw new ArgumentException("New password and confirmation do not match.");
            }

            var user = await _userRepository.GetUserById(passDTO.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(passDTO.Password, user.PasswordHash))
            {
                throw new ArgumentException("Mật khẩu hiện tại không đúng.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passDTO.NewPassword);
            await _userRepository.UpdateUser(user);
        }

        public async Task<List<User>> GetUsersByEmailOrPhoneAsync(string search)
        {
            return await _userRepository.GetUserByEmailOrPhone(search);
        }


    }
}
