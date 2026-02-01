using DREAMHOMES.Models;
using DREAMHOMES.Services.Interfaces;

namespace DREAMHOMES.Services
{
    public class DocumentService : IDocumentService
    {
        public async Task AssignDocuments(IList<IFormFile> files, string email, SellerInformation sellerInformation)
        {
            sellerInformation.Documents = [];
            foreach (var file in files)
            {
                var basePath = Path.Combine("Documents");
                bool basePathExists = Directory.Exists(basePath);

                if (!basePathExists)
                {
                    Directory.CreateDirectory(basePath);
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
