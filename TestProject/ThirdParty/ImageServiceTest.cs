using API.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Moq;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Reflection;

namespace TestProject.ThirdParty
{
    [TestFixture]
    public class ImageServiceTest
    {
        private Mock<ICloudinaryService> _mockCloudinary;
        private ImageService _imageService;
        private Mock<HttpClient> _mockHttpClient;
        [SetUp]
        public void Setup()
        {
            var account = new Account(
                 "fake_cloud_name",
                 "fake_api_key",
                 "fake_api_secret"
             );

            _mockCloudinary = new Mock<ICloudinaryService>();
            _imageService = new ImageService(_mockCloudinary.Object);
            _mockHttpClient = new Mock<HttpClient>();
        }
        [Test]
        public void UploadImageAsync_CloudinaryError_ThrowsException()
        {
            // Arrange
            var file = CreateTestFormFile("test.jpg", 1024, "image/jpeg");
            _mockCloudinary.Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
                .ThrowsAsync(new Exception("Cloudinary error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _imageService.UploadImagetAsync(file));
            Assert.That(ex.Message, Does.Contain("Image upload failed"));
        }
        [Test]
        public async Task UploadImageAsync_ValidFile_ReturnsUploadResult()
        {
            // Arrange
            var file = CreateTestFormFile("test.jpg", 1024, "image/jpeg");
            var expectedResult = new ImageUploadResult { PublicId = "test_id" };

            // Setup mock interface
            _mockCloudinary.Setup(c =>
                c.UploadAsync(It.IsAny<ImageUploadParams>())
            ).ReturnsAsync(expectedResult);

            // Act
            var result = await _imageService.UploadImagetAsync(file);

            // Assert
            Assert.That(result.PublicId, Is.EqualTo(expectedResult.PublicId));
        }
        [Test]
        public void UploadImageAsync_InvalidFileType_ThrowsInvalidImageException()
        {
            // Arrange
            var file = CreateTestFormFile("invalid.txt", 1024, "text/plain");

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidImageException>(() =>
                _imageService.UploadImagetAsync(file)
            );
            Assert.That(ex.Message, Does.Contain("định dạng file ảnh"));
        }
        // Helper Methods
        private IFormFile CreateTestFormFile(string fileName, long fileSize, string contentType)
        {
            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream(new byte[fileSize]);

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(fileSize);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

            return fileMock.Object;
        }
        [Test]
        public void DeleteImageAsync_CloudinaryError_ThrowsException()
        {
            // Arrange
            var publicId = "valid_id";
            var expectedResult = new DeletionResult { Result = "error", Error = new Error { Message = "Failed" } };
            _mockCloudinary.Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>())).ReturnsAsync(expectedResult);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _imageService.DeleteImageAsync(publicId));
            Assert.That(ex.Message, Does.Contain("Cloudinary error: Failed"));
        }
        [Test]
        public async Task DeleteImageAsync_ValidPublicId_ReturnsOk()
        {
            // Arrange
            var publicId = "valid_id";
            var expectedResult = new DeletionResult { Result = "ok" };

            _mockCloudinary.Setup(c =>
                c.DestroyAsync(It.IsAny<DeletionParams>())
            ).ReturnsAsync(expectedResult);

            // Act
            var result = await _imageService.DeleteImageAsync(publicId);

            // Assert
            Assert.That(result.Result, Is.EqualTo("ok"));
        }
        [Test]
        public void DeleteImageAsync_EmptyPublicId_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() =>
                _imageService.DeleteImageAsync("")
            );
            Assert.That(ex.Message, Does.Contain("không hợp lệ"));
        }
        [Test]
        public async Task ReadSmallQrCode_ValidQrImage_ReturnsCorrectText()
        {
            // Arrange
            var expectedText = "test example";
            var qrFile =GetRealQrCodeFile();

            // Act
            var result = await _imageService.ReadSmallQrCode(qrFile);

            // Assert
            Assert.That(result, Is.EqualTo(expectedText));
        }

        [Test]
        public void ReadQrCodeUrl_InvalidUrl_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _imageService.ReadQrCodeUrl("invalid-url")
            );
            Assert.That(ex.Message, Does.Contain("không đúng định dạng"));
        }
        [Test]
        public void ValidateImage_ValidFile_DoesNotThrow()
        {
            // Arrange
            var file = CreateTestFormFile("valid.png", 4 * 1024 * 1024, "image/png");

            // Act & Assert
            Assert.DoesNotThrow(() => _imageService.ValidateImage(file));
        }

        [Test]
        public void ValidateImage_OverSizeFile_ThrowsException()
        {
            // Arrange
            var file = CreateTestFormFile("large.jpg", 6 * 1024 * 1024, "image/jpeg");

            // Act & Assert
            var ex = Assert.Throws<InvalidImageException>(() =>
                _imageService.ValidateImage(file)
            );
            Assert.That(ex.Message, Does.Contain("5MB"));
        }
        [Test]
        public void ValidateImage_NullFile_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidImageException>(() => _imageService.ValidateImage(null));
            Assert.That(ex.Message, Does.Contain("No file provided"));
        }
        [Test]
        public void ReadSmallQrCode_NoQrCode_ThrowsException()
        {
            // Arrange
            var file = CreateTestImageFile("no_qr.png", "image/png"); // Tạo ảnh trắng

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidImageException>(() => _imageService.ReadSmallQrCode(file));
            Assert.That(ex.Message, Does.Contain("Không thể đọc QR code"));
        }

        private IFormFile CreateTestImageFile(string fileName, string contentType)
        {
            using var image = new Image<Rgba32>(100, 100); // Ảnh trắng 100x100
            var memoryStream = new MemoryStream();
            image.SaveAsPng(memoryStream);
            memoryStream.Position = 0;

            return new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
        private IFormFile GetRealQrCodeFile()
        {
            // Lấy thư mục gốc của ứng dụng
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // Quay lại 3 cấp thư mục
            var threeLevelsUp = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));

            // Sử dụng đường dẫn tương đối đến file PNG trong thư mục Resources
            var relativePath = @"Resources\qrcode.png";
            var filePath = Path.Combine(threeLevelsUp, relativePath);

            // Đọc nội dung file
            byte[] fileBytes;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fileStream.Length];
                fileStream.Read(fileBytes, 0, (int)fileStream.Length);
            }

            // Tạo MemoryStream từ byte array
            var memoryStream = new MemoryStream(fileBytes);

            // Tạo đối tượng IFormFile
            var file = new FormFile(
                baseStream: memoryStream,
                baseStreamOffset: 0,
                length: fileBytes.Length,
                name: "qrFile",
                fileName: "test_qr.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            // Thiết lập Content-Type
            file.Headers["Content-Type"] = "image/png";

            return file;
        }

    }

}
