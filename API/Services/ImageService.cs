using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Drawing;
using ZXing.Common;
using ZXing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using ZXing.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace API.Services
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams);
        Task<DeletionResult> DestroyAsync(DeletionParams deleteParams);
    }
    public class CloudinaryWrapper : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryWrapper(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams)
        {
            return await _cloudinary.UploadAsync(uploadParams);
        }

        public async Task<DeletionResult> DestroyAsync(DeletionParams deleteParams)
        {
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
    public class InvalidImageException : Exception
    {
        public InvalidImageException(string message) : base(message) { }
    }

    public class ImageService
    {
        private readonly ICloudinaryService _cloudinary;
        public ImageService(ICloudinaryService cloudinary)
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
                    Invalidate = true 
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result != "ok")
                    throw new Exception($"Cloudinary error: {result.Error?.Message}");

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể xóa ảnh: {ex.Message}");
            }
        }


        public async Task<string> ReadSmallQrCode(IFormFile file)
        {
            ValidateImage(file);
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            try
            {
                // Load có thể ném UnknownImageFormatException, ImageFormatException, ...
                using var image = await Image.LoadAsync<Rgba32>(memoryStream);

                image.Mutate(ctx => ctx
                    .Resize(new ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(image.Width * 2, image.Height * 2),
                        Mode = ResizeMode.Stretch,
                        Sampler = KnownResamplers.Lanczos3
                    })
                    .GaussianSharpen(3)
                );

                // Thử đọc QR code
                return TryReadQrCode(image);
            }
            catch (ImageFormatException ex)
            {
                throw new InvalidImageException("Định dạng ảnh không được hỗ trợ hoặc file đã bị corrupt.");
            }
        }
        public async Task<string> ReadQrCodeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL không hợp lệ");

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new ArgumentException("URL không đúng định dạng");

            using var httpClient = new HttpClient();
            byte[] imageBytes;

            try
            {
                imageBytes = await httpClient.GetByteArrayAsync(url);
            }
            catch (Exception ex)
            {
                throw new InvalidImageException($"Không thể tải ảnh từ URL: {ex.Message}");
            }

            if (imageBytes.Length > 5 * 1024 * 1024)
                throw new InvalidImageException("File có dung lượng vượt quá 5MB limit");

            try
            {
                using var stream = new MemoryStream(imageBytes);
                using var image = await Image.LoadAsync<Rgba32>(stream);

                image.Mutate(ctx => ctx
                    .Resize(new ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(image.Width * 2, image.Height * 2),
                        Mode = ResizeMode.Stretch,
                        Sampler = KnownResamplers.Lanczos3
                    })
                    .GaussianSharpen(3)
                );

                return TryReadQrCode(image);
            }
            catch (Exception ex)
            {
                throw new InvalidImageException( ex.Message);
            }
        }
        private string TryReadQrCode(Image<Rgba32> image)
        {
            try
            {
                var reader = new ZXing.ImageSharp.BarcodeReader<Rgba32>
                {
                    Options = new DecodingOptions
                    {
                        PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                        TryHarder = true,
                        TryInverted = true
                    }
                };

                var luminanceSource = new ImageSharpLuminanceSource<Rgba32>(image);
                var result = reader.Decode(luminanceSource);
                if (result == null)
                {
                    throw new Exception();
                }
                return result.Text;
            }
            catch (InvalidImageException e)
            {
                throw new InvalidImageException("Không tìm thấy QR code trong ảnh");
            }
            catch (Exception ex)
            {
                throw new InvalidImageException("Không thể đọc QR code: " + ex.Message);
            }   
        }

        public void ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidImageException("No file provided");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidImageException("File có dung lượng vượt quá 5MB limit");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidImageException($"Lỗi định dạng file ảnh. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
        }

    }
}