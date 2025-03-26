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


        public async Task<string> ReadSmallQrCode(IFormFile file)
        //    {
            ValidateImage(file);
        //        await file.CopyToAsync(memoryStream);
        //        memoryStream.Position = 0;

        //        using var image = await Image.LoadAsync<Rgba32>(memoryStream);
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
                    Mode = ResizeMode.Stretch,
            // Thử đọc QR code
            return TryReadQrCode(image);
        }
            {
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
                using var croppedImage = originalImage.Clone(ctx => ctx.Crop(cropRect));

                // Phóng to vùng cắt 200%
                croppedImage.Mutate(ctx => ctx
                    .Resize(new ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(croppedImage.Width * 2, croppedImage.Height * 2),
                        Mode = ResizeMode.Stretch,
                        Sampler = KnownResamplers.Lanczos3
                    })
                    .GaussianSharpen(3)
                );

                result = ReadQrCodeFromImage(croppedImage);
                if (!string.IsNullOrEmpty(result) && result != "Không tìm thấy QR code")
                {
                    return result;
                }
            }

            return "Không tìm thấy QR code";
        }

        private string ReadQrCodeFromImage(Image<Rgba32> image)
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
            catch(Exception e)
            {
                throw new Exception("Không tìm thấy QR code trong ảnh",e);
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
