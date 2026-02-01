using DREAMHOMES.Models;

namespace DREAMHOMES.Services.Interfaces
{
    public interface IDocumentService
    {
        public Task AssignDocuments(IList<IFormFile> files, string email, SellerInformation sellerInformation);
    }
}
