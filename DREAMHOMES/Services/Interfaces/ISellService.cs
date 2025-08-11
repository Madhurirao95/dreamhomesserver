using DREAMHOMES.Models;
using System.ComponentModel.DataAnnotations;

namespace DREAMHOMES.Services.Interfaces
{
    public interface ISellService
    {
        Task<IEnumerable<ValidationResult>> PostListing(SellerInformation sellerInformation);

        Task<IEnumerable<ValidationResult>> UpdateListing(SellerInformation existingSellerInformation, SellerInformation sellerInformation);

        Task<IEnumerable<SellerInformation>> GetAllListingBySeller(string userId);

        Task<SellerInformation> GetSellerInformationById(int id);

        Task<(IEnumerable<SellerInformation>, int)> GetAllListingByCoordinates(double coordinatex, double coordinatey, int page, int pageSize);
    }
}
