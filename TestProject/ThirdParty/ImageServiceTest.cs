using API.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZXing.QrCode.Internal;
using ZXing.QrCode;
using ZXing;

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

        private IFormFile GenerateQrCodeFile(string text)
        {
            var writer = new BarcodeWriter<Image<Rgba32>>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 200,
                    Height = 200,
                    Margin = 1,
                    ErrorCorrection = ErrorCorrectionLevel.H
                }
            };

            using var image = writer.Write(text);
            var ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("qrcode.png");
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

            return fileMock.Object;
        }

        private HttpClient CreateMockHttpClient(byte[] content)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(content)
                });

            return new HttpClient(handler.Object);
        }
    }

}
