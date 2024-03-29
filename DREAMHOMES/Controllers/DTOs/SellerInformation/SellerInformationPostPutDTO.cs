namespace DREAMHOMES.Controllers.DTOs.SellerInformation
{
    /// <summary>
    /// Represents the data related to Seller's Listing.
    /// </summary>
    public class SellerInformationPostPutDTO : SellerInformationDTO
    {
        /// <summary>
        /// Gets or sets the list of Photos/Videos associated to the Listing.
        /// </summary>
        public IList<IFormFile> Documents { get; set; } = null!;
    }
}
