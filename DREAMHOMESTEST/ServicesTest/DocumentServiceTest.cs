using DREAMHOMES.Models;
using DREAMHOMES.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace DREAMHOMESTEST.ServicesTest
{
    [TestFixture]
    public class DocumentServiceTest
    {
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private Mock<ILogger<DocumentService>> _mockLogger;
        private DocumentService _documentService;

        [SetUp]
        public void SetUp()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<DocumentService>>();

            _documentService = new DocumentService(
                _mockEnvironment.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task AssignDocuments_WithValidFiles_ShouldAddDocumentsToSellerInformation()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var fileContent = Encoding.UTF8.GetBytes("Test file content");
            var mockFile = CreateMockFormFile("testfile.pdf", "application/pdf", fileContent);
            var files = new List<IFormFile> { mockFile };
            var email = "user@example.com";
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, email, sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents.Count, Is.EqualTo(1));
            Assert.That(sellerInfo.Documents[0].Name, Is.EqualTo("testfile.pdf"));
            Assert.That(sellerInfo.Documents[0].FileType, Is.EqualTo("application/pdf"));
            Assert.That(sellerInfo.Documents[0].AuthorName, Is.EqualTo(email));
            Assert.That(sellerInfo.Documents[0].Extension, Is.EqualTo(".pdf"));
        }

        [Test]
        public async Task AssignDocuments_WithMultipleFiles_ShouldAddAllDocuments()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var files = new List<IFormFile>
            {
                CreateMockFormFile("file1.pdf", "application/pdf", Encoding.UTF8.GetBytes("Content 1")),
                CreateMockFormFile("file2.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Encoding.UTF8.GetBytes("Content 2")),
                CreateMockFormFile("file3.txt", "text/plain", Encoding.UTF8.GetBytes("Content 3"))
            };
            
            var email = "user@example.com";
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, email, sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents.Count, Is.EqualTo(3));
            Assert.That(sellerInfo.Documents[0].Name, Is.EqualTo("file1.pdf"));
            Assert.That(sellerInfo.Documents[1].Name, Is.EqualTo("file2.docx"));
            Assert.That(sellerInfo.Documents[2].Name, Is.EqualTo("file3.txt"));
        }

        [Test]
        public async Task AssignDocuments_WithEmptyFileList_ShouldNotAddDocuments()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var files = new List<IFormFile>();
            var email = "user@example.com";
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, email, sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task AssignDocuments_ShouldSetDocumentPropertiesCorrectly()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var fileContent = Encoding.UTF8.GetBytes("Document content");
            var mockFile = CreateMockFormFile("document.pdf", "application/pdf", fileContent);
            var files = new List<IFormFile> { mockFile };
            var email = "author@example.com";
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, email, sellerInfo);

            // ASSERT
            var document = sellerInfo.Documents[0];
            Assert.That(document.Name, Is.EqualTo("document.pdf"));
            Assert.That(document.AuthorName, Is.EqualTo("author@example.com"));
            Assert.That(document.FileType, Is.EqualTo("application/pdf"));
            Assert.That(document.Extension, Is.EqualTo(".pdf"));
            Assert.That(document.Size, Is.EqualTo(fileContent.Length));
            Assert.That(document.DateCreated, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(document.DateModified, Is.Not.EqualTo(DateTime.MinValue));
        }

        [Test]
        public async Task AssignDocuments_WithDifferentFileTypes_ShouldPreserveMimeTypes()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var files = new List<IFormFile>
            {
                CreateMockFormFile("image.jpg", "image/jpeg", Encoding.UTF8.GetBytes("image")),
                CreateMockFormFile("document.pdf", "application/pdf", Encoding.UTF8.GetBytes("pdf")),
                CreateMockFormFile("spreadsheet.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Encoding.UTF8.GetBytes("xlsx"))
            };
            
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, "user@example.com", sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents[0].FileType, Is.EqualTo("image/jpeg"));
            Assert.That(sellerInfo.Documents[1].FileType, Is.EqualTo("application/pdf"));
            Assert.That(sellerInfo.Documents[2].FileType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        }

        [Test]
        public async Task AssignDocuments_WithFileNoExtension_ShouldHandleEmptyExtension()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var mockFile = CreateMockFormFile("noextension", "text/plain", Encoding.UTF8.GetBytes("content"));
            var files = new List<IFormFile> { mockFile };
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, "user@example.com", sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents[0].Extension, Is.EqualTo(""));
        }

        [Test]
        public async Task AssignDocuments_ShouldClearExistingDocuments()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var mockFile = CreateMockFormFile("newfile.pdf", "application/pdf", Encoding.UTF8.GetBytes("new"));
            var files = new List<IFormFile> { mockFile };
            
            var sellerInfo = new SellerInformation
            {
                Documents = new List<Document>
                {
                    new Document { Name = "oldfile.pdf" }
                }
            };

            // ACT
            await _documentService.AssignDocuments(files, "user@example.com", sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents.Count, Is.EqualTo(1));
            Assert.That(sellerInfo.Documents[0].Name, Is.EqualTo("newfile.pdf"));
        }

        [Test]
        public async Task AssignDocuments_WithLargeFile_ShouldStoreCorrectFileSize()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var largeContent = new byte[1024 * 100];
            var mockFile = CreateMockFormFile("largefile.bin", "application/octet-stream", largeContent);
            var files = new List<IFormFile> { mockFile };
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, "user@example.com", sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents[0].Size, Is.EqualTo(1024 * 100));
        }

        [Test]
        public async Task AssignDocuments_ShouldSetCorrectAuthorNameForAllDocuments()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var files = new List<IFormFile>
            {
                CreateMockFormFile("file1.pdf", "application/pdf", Encoding.UTF8.GetBytes("1")),
                CreateMockFormFile("file2.pdf", "application/pdf", Encoding.UTF8.GetBytes("2"))
            };
            
            var email = "specific@example.com";
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, email, sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents[0].AuthorName, Is.EqualTo(email));
            Assert.That(sellerInfo.Documents[1].AuthorName, Is.EqualTo(email));
        }

        [Test]
        public async Task AssignDocuments_ShouldAddFilepathToDocuments()
        {
            // ARRANGE
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            
            var mockFile = CreateMockFormFile("test.pdf", "application/pdf", Encoding.UTF8.GetBytes("test"));
            var files = new List<IFormFile> { mockFile };
            var sellerInfo = new SellerInformation();

            // ACT
            await _documentService.AssignDocuments(files, "user@example.com", sellerInfo);

            // ASSERT
            Assert.That(sellerInfo.Documents[0].FilePath, Is.Not.Null);
            Assert.That(sellerInfo.Documents[0].FilePath, Is.Not.Empty);
            Assert.That(sellerInfo.Documents[0].FilePath, Does.Contain("test.pdf"));
        }

        private IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
        {
            var mockFile = new Mock<IFormFile>();
            
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(content.Length);
            
            var stream = new MemoryStream(content);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream s, CancellationToken ct) => s.WriteAsync(content, 0, content.Length));
            
            return mockFile.Object;
        }
    }
}
