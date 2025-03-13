using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

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
