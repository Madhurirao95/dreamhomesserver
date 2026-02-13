using DREAMHOMES.Models;
using DREAMHOMES.Services.Interfaces;

namespace DREAMHOMES.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IWebHostEnvironment environment,
            ILogger<DocumentService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task AssignDocuments(IList<IFormFile> files, string email, SellerInformation sellerInformation)
        {
            sellerInformation.Documents = [];
            foreach (var file in files)
            {
                string basePath;

                if (_environment.IsDevelopment())
                {
                    // Development: Save in project Documents folder
                    basePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
                }
                else
                {
                    // Production (Azure): Save in persistent storage
                    basePath = Path.Combine("/home/site/wwwroot", "Documents");
                }

                // Create directory if it doesn't exist
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    _logger.LogInformation($"Created directory: {basePath}");
                }

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var filePath = Path.Combine(basePath, file.FileName);
                var extension = Path.GetExtension(file.FileName);

                if (!System.IO.File.Exists(filePath))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }


                var document = new Document
                {
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    Name = file.FileName,
                    FilePath = filePath,
                    FileType = file.ContentType,
                    Extension = extension,
                    Size = file.Length,
                    AuthorName = email
                };
                sellerInformation.Documents.Add(document);
            }
        }
    }
}
