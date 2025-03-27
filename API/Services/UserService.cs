using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet.Core;
using DataAccess.DTOs;
using DataAccess.DTOs.Auth;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using DataAccess.Repositories.StationRepo;
using System.Text.RegularExpressions;

namespace API.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICccdRepository _cccdRepository;
        private readonly IDriverLicenseRepository _licenseRepository;
        private readonly ImageService _imageService;
        private readonly OtpServices _otpServices;
        public UserService(IUserRepository userRepository, ICccdRepository cccdRepository, ImageService imageService, OtpServices otpServices, IDriverLicenseRepository licenseRepository)
        {
            _userRepository = userRepository;
            _cccdRepository = cccdRepository;
            _imageService = imageService;
            _otpServices = otpServices;
            _licenseRepository = licenseRepository;
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
            if (!IsPasswordCorrect(passDTO.NewPassword))
            {
                throw new ArgumentException("Password is not strong enough");
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
        public async Task AddDriverLicenseAsync(int userId, DriverLicenseRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Request cannot be null");
            }
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            ImageUploadResult frontUpload = null;
            ImageUploadResult backUpload = null;
            var existingLicenses = await _licenseRepository.GetLicensesByUserId(userId);
            if (existingLicenses != null)
            {
                foreach (var license in existingLicenses)
                {
                    if (license.Code == request.LicenseNumber)
                    {
                        if (license.Status == "Inactive")
                        {

                            license.Status = "Active";
                            license.Class = request.Class;
                            license.UpdateAt = DateTime.UtcNow;
                           
                            await _licenseRepository.UpdateLicense(license);
                            return;
                        }
                        else
                        {
                            throw new ArgumentException("License already exists");
                        }
                    }
                }
            }
            try
            {
                _imageService.ValidateImage(request.LicenseFrontImage);
                _imageService.ValidateImage(request.LicenseBackImage);
                frontUpload = await _imageService.UploadImagetAsync(request.LicenseFrontImage);
                backUpload = await _imageService.UploadImagetAsync(request.LicenseBackImage);
                var license = new DriverLicense
                {
                    UserId = userId,
                    Code = request.LicenseNumber,
                    Class = request.Class,
                    ImgFront = frontUpload.Url.ToString(),
                    ImgFrontPubblicId = frontUpload.PublicId,
                    ImgBack = backUpload.Url.ToString(),
                    ImgBackPubblicId = backUpload.PublicId,
                    Status = "Active",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };
                using var transaction = await _licenseRepository.BeginTransactionAsync();
                try
                {
                    await _licenseRepository.SaveLicense(license);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                var deleteTasks = new List<Task>();
                if (frontUpload != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(frontUpload.PublicId));
                if (backUpload != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(backUpload.PublicId));
                await Task.WhenAll(deleteTasks);
                throw new Exception("Add license failed", ex);
            }

        }
        public async Task UpdateDriverLiscense(string licensecode, DriverLicenseRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Request cannot be null");
            }
            var license = await _licenseRepository.GetLicenseByCode(licensecode);
            if (license == null)
            {
                throw new ArgumentException("License not found");
            }
            ImageUploadResult newFront = null;
            ImageUploadResult newBack = null;
            var oldFront = license.ImgFrontPubblicId;
            var oldBack = license.ImgBackPubblicId;
            try
            {
                if (request.LicenseFrontImage != null)
                {
                    _imageService.ValidateImage(request.LicenseFrontImage);
                    newFront = await _imageService.UploadImagetAsync(request.LicenseFrontImage);
                }

                if (request.LicenseBackImage != null)
                {
                    _imageService.ValidateImage(request.LicenseBackImage);
                    newBack = await _imageService.UploadImagetAsync(request.LicenseBackImage);
                }


                // Cập nhật thông tin
                license.Code = request.LicenseNumber;
                license.Class = request.Class;
                license.UpdateAt = DateTime.UtcNow;
                if (newFront != null)
                {
                    license.ImgFront = newFront.Url.ToString();
                    license.ImgFrontPubblicId = newFront.PublicId;
                }

                if (newBack != null)
                {
                    license.ImgBack = newBack.Url.ToString();
                    license.ImgBackPubblicId = newBack.PublicId;
                }
                using var transaction = await _licenseRepository.BeginTransactionAsync();
                try
                {
                    await _licenseRepository.UpdateLicense(license);
                    // Xóa ảnh cũ sau khi update thành công
                    var deleteTasks = new List<Task>();
                    if (newFront != null) deleteTasks.Add(_imageService.DeleteImageAsync(oldFront));
                    if (newBack != null) deleteTasks.Add(_imageService.DeleteImageAsync(oldBack));

                    await Task.WhenAll(deleteTasks);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                var deleteTasks = new List<Task>();
                if (newFront != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(newFront.PublicId));
                if (newBack != null)
                    deleteTasks.Add(_imageService.DeleteImageAsync(newBack.PublicId));
                await Task.WhenAll(deleteTasks);
                throw new Exception("Update license failed", ex);
            }
        }

        //public async Task<IEnumerable<DriverLicenseResponse>> GetDriverLicensesAsync(int userid)
        //{
        //    var licenses = await _licenseRepository.GetLicensesByUserId(userid);
        //    if (licenses == null || !licenses.Any())
        //    {
        //        return Enumerable.Empty<DriverLicenseResponse>();


        //    }
        //    return licenses.Select(l => new DriverLicenseResponse
        //    {
        //        LicenseNumber = l.Code,
        //        Class = l.Class,
        //        FrontImageUrl = l.ImgFront,
        //        BackImageUrl = l.ImgBack,
        //        Status = l.Status,
        //        CreatedAt = l.CreateAt,
        //        UpdatedAt = l.UpdateAt
        //    });
        //}
        public async Task<IEnumerable<DriverLicenseResponse>> GetActiveDriverLicensesAsync(int userId)
        {
            var licenses = await _licenseRepository.GetLicensesByUserId(userId);
            if (licenses == null)
            {
                return Enumerable.Empty<DriverLicenseResponse>();
            }

            var result = new List<DriverLicenseResponse>();

            foreach (var license in licenses.Where(l => l.Status == "Active"))
            {
                try
                {
                    var qrContent = await _imageService.ReadQrCodeUrl(license.ImgBack);
                    result.Add(new DriverLicenseResponse(license, qrContent));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing license {license.Code}: {ex.Message}");
                    result.Add(new DriverLicenseResponse(license, ""));
                }
            }

            return result;
        }

        public async Task DeleteDriverLicenseAsync(string licenseCode)
        {
            var license = await _licenseRepository.GetLicenseByCode(licenseCode);
            if (license == null)
            {
                throw new ArgumentException("License not found");
            }

            license.Status = "Inactive";
            license.UpdateAt = DateTime.UtcNow;

            using var transaction = await _licenseRepository.BeginTransactionAsync();
            try
            {
                await _licenseRepository.UpdateLicense(license);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public PagedResult<UserDto> GetUsers(string? searchQuery, string? status, int? roleId, int pageNumber, int pageSize)
        {
            return _userRepository.GetUsers(searchQuery, status, roleId, pageNumber, pageSize);
        }

        public async Task ChangeUserStatusAsync(int userId, string newStatus)
        {
            await _userRepository.ChangeUserStatusAsync(userId, newStatus);
        }

        public PagedResult<FeedbackDto> GetFeedbacks(string? search, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            return _userRepository.GetFeedbacks(search, startDate, endDate, page, pageSize);
        }

        public async Task<List<Feedback>> GetFeedbackByUserId(int userId)
        {
            return await _userRepository.GetFeedbackByUserId(userId);
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
    }
}
