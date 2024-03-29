using SPOTAHOME.Models;
using System.ComponentModel.DataAnnotations;

namespace SPOTAHOME.Services.Interfaces
{
    public interface ISellService
    {
        Task<IEnumerable<ValidationResult>> PostListing(SellerInformation sellerInformation);

        Task<IEnumerable<ValidationResult>> UpdateListing(SellerInformation existingSellerInformation, SellerInformation sellerInformation);

        Task<IEnumerable<SellerInformation>> GetAllListingBySeller(string userId);

        Task<SellerInformation> GetSellerInformationById(int id);
    }
}
