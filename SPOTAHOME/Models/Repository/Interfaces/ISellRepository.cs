using static SPOTAHOME.Models.SellerInformation;

namespace SPOTAHOME.Models.Repository.Interfaces
{
    public interface ISellRepository : IDataRepository<SellerInformation>
    {
        Task<IEnumerable<SellerInformation>> GetAllByAddress(string streetAddress, string city, string zipCode, string country, States state);

        Task<IEnumerable<SellerInformation>> GetAllListingBySeller(string userId);
    }
}
