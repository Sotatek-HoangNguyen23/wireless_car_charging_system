using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Drawing;
using ZXing.Common;
using ZXing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using ZXing.ImageSharp;

namespace API.Services
{
    public class InvalidImageException : Exception
    {
        public InvalidImageException(string message) : base(message) { }
    }

    public class ImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
   
        }

        public async Task<ImageUploadResult> UploadImagetAsync(IFormFile file)
        {
            ValidateImage(file);
            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = Guid.NewGuid().ToString(),
                    Overwrite = true,
                    Transformation = new Transformation()
                        .Width(800)
                        .Height(600)
                        .Crop("fill")
                        .Quality("auto"),
                };

                return await _cloudinary.UploadAsync(uploadParams);
            }
            catch (Exception ex)
            {
                throw new Exception("Image upload failed", ex);
            }
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicId))
                    throw new ArgumentException("Public ID không hợp lệ");

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image,
                    Invalidate = true // Xóa cache CDN
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result != "ok")
                    throw new Exception($"Cloudinary error: {result.Error?.Message}");

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể xóa ảnh: {ex.Message}", ex);
            }
        }
        public async Task<string> ReadQrCodeAsync(IFormFile file)
        {
            ValidateImage(file);

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var image = await Image.LoadAsync<Rgba32>(stream);

                var result = await Task.Run(() =>
                {
                    // Sử dụng fully qualified name để tránh ambiguity
                    var reader = new ZXing.ImageSharp.BarcodeReader<Rgba32>
                    {
                        Options = new DecodingOptions
                        {
                            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                            TryHarder = true
                        }
                    };

                    // Tạo luminance source và decode
                    var luminanceSource = new ImageSharpLuminanceSource<Rgba32>(image);
                    return reader.Decode(luminanceSource);
                }).ConfigureAwait(false);

                return result?.Text ?? "Không tìm thấy QR code";
            }
            catch (Exception ex)
            {
                throw new InvalidImageException($"Lỗi đọc QR code: {ex.Message}");
            }
        }
        public void ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidImageException("No file provided");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidImageException("File size exceeds 5MB limit");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidImageException($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

    }
}
