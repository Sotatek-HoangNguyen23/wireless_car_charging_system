using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet.Core;
using DataAccess.DTOs.Auth;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using System.Text.RegularExpressions;

namespace API.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICccdRepository _cccdRepository;
        private readonly ImageService _imageService;
        private readonly OtpServices _otpServices;
        public UserService(IUserRepository userRepository, ICccdRepository cccdRepository, ImageService imageService, OtpServices otpServices)
        {
            _userRepository = userRepository;
            _cccdRepository = cccdRepository;
            _imageService = imageService;
            _otpServices = otpServices;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Request không được null");
            }

            var existingEmail = await _userRepository.GetUserByEmail(request.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email đã tồn tại");
            }
            var existingCccd = await _cccdRepository.GetCccdByCode(request.CccdCode.Trim());
            if (existingCccd != null)
            {
                throw new InvalidOperationException("CCCD đã tồn tại");
            }
            var existingPhone = await _userRepository.GetUserByPhone(request.PhoneNumber);
            if (existingPhone != null)
            {
                throw new InvalidOperationException("Số điện thoại đã tồn tại");
            }

            // Kiểm tra độ mạnh của mật khẩu
            if (!IsPasswordCorrect(request.PasswordHash))
            {
                throw new ArgumentException("Mật khẩu không đủ mạnh");
            }

            ImageUploadResult? frontUploadResult = null;
            ImageUploadResult? backUploadResult = null;

            try
            {
                // Xác thực ảnh trước khi upload
                _imageService.ValidateImage(request.CCCDFrontImage);
                _imageService.ValidateImage(request.CCCDBackImage);

                // Upload ảnh song song
                var frontUploadTask = _imageService.UploadImagetAsync(request.CCCDFrontImage);
                var backUploadTask = _imageService.UploadImagetAsync(request.CCCDBackImage);
                await Task.WhenAll(frontUploadTask, backUploadTask);

                frontUploadResult = await frontUploadTask;
                backUploadResult = await backUploadTask;
                // Mã hóa mật khẩu
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);

                // Khởi tạo đối tượng người dùng
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = hashedPassword,
                    Fullname = request.Fullname,
                    PhoneNumber = request.PhoneNumber,
                    Dob = request.Dob,
                    RoleId = 1,
                    Gender = request.Gender,
                    Address = request.Address,
                    Status = "Active",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                // Sử dụng transaction để đảm bảo tính toàn vẹn của dữ liệu
                using var transaction = await _userRepository.BeginTransactionAsync();
                try
                {
                    // Lưu thông tin người dùng
                    await _userRepository.SaveUser(user);

                    // Tạo thông tin CCCD
                    var cccd = new Cccd
                    {
                        UserId = user.UserId,
                        Code = request.CccdCode.Trim(),
                        ImgFront = frontUploadResult.Url.ToString(),
                        ImgFrontPubblicId = frontUploadResult.PublicId,
                        ImgBack = backUploadResult.Url.ToString(),
                        ImgBackPubblicId = backUploadResult.PublicId,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };

                    await _cccdRepository.SaveCccd(cccd);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Ném lỗi với thông báo chi tiết cho biết thất bại trong quá trình lưu dữ liệu
                    throw new Exception("Đăng ký thất bại trong quá trình lưu dữ liệu", ex);
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xảy ra, tiến hành xóa ảnh đã upload (nếu có)
                var deleteTasks = new List<Task>();
                if (frontUploadResult != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(frontUploadResult.PublicId));
                if (backUploadResult != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(backUploadResult.PublicId));

                await Task.WhenAll(deleteTasks);
                // Ném lỗi với thông báo tổng quát, bao gồm inner exception để dễ dàng debug
                throw new Exception("Đăng ký thất bại", ex);
            }
        }

        public async Task<UserDto> GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email không thể trống");
            }
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new ArgumentException("Khong tìm thấy người dùng");
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
        public async Task<UserDto> GetUserByCccd(string cccd)
        {
            if (string.IsNullOrEmpty(cccd))
            {
                throw new ArgumentException("Cccd không thể trống");
            }
            var user = await _userRepository.GetUserByCccd(cccd);
            if (user == null)
            {
                throw new ArgumentException("Khong tìm thấy người dùng");
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
        public async Task<UserDto> GetUserByPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                throw new ArgumentException("Phone không thể trống");
            }
            var user = await _userRepository.GetUserByPhone(phone);
            if (user == null)
            {
                throw new ArgumentException("Khong tìm thấy người dùng");
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
        public bool IsEmailCorrect(string email)
        {
            string regex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, regex);
        }
        public bool IsPhoneCorrect(string phone)
        {
            string regex = @"^(0)(3[2-9]|5[2689]|7[06789]|8[1-9]|9\d|2[0-9])\d{7}$";
            return Regex.IsMatch(phone, regex);
        }
        public bool IsPasswordCorrect(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var hasMinimumLength = password.Length >= 6;
            var hasUpperCase = password.Any(char.IsUpper);
            var hasNumber = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            var allowedSpecialChars = "!@#$%^&*(),.?\":{}|<>";
            var hasValidSpecialChar = password.Any(c => allowedSpecialChars.Contains(c));

            return hasMinimumLength &&
                   hasUpperCase &&
                   hasNumber &&
                   hasSpecialChar &&
                   hasValidSpecialChar;
        }
        public async Task<bool> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException("Request cannot be null");
                }
                var user = await _userRepository.GetUserByEmail(request.Email);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }
                var isValid = await _otpServices.verifyResetPasswordToken(request.Token, request.Email);
                if (!isValid)
                {
                    throw new ArgumentException("Invalid Token");
                }
                if (!IsPasswordCorrect(request.NewPassword))
                {
                    throw new ArgumentException("Password is not strong enough");
                }
                var password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.PasswordHash = password;
                user.UpdateAt = DateTime.UtcNow;
                await _userRepository.UpdateUser(user);
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception("Reset password failed", ex);
            }
        }
    }
}
