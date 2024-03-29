using static DREAMHOMES.Models.SellerInformation;

namespace DREAMHOMES.Models.Repository.Interfaces
{
    public interface ISellRepository : IDataRepository<SellerInformation>
    {
        Task<IEnumerable<SellerInformation>> GetAllByAddress(string streetAddress, string city, string zipCode, string country, States state);

        Task<IEnumerable<SellerInformation>> GetAllListingBySeller(string userId);
    }
}
