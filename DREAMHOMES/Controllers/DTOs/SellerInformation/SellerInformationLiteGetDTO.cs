using DREAMHOMES.Controllers.DTOs.SellerInformation.Document;

namespace DREAMHOMES.Controllers.DTOs.SellerInformation
{
    /// <summary>
    /// Resprests a structure that sends minimal data regarding <see cref="SellerInformation"/>
    /// </summary>
    public class SellerInformationLiteGetDTO : SellerInformationGetDTO
    {
        /// <summary>
        /// Gets or sets a random document associated to the Seller.
        /// </summary>
        public DocumentLiteDTO RandomDocument { get; set; } = null!;
    }
}
