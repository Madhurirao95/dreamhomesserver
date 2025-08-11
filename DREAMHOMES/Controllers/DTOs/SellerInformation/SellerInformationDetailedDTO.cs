using DREAMHOMES.Controllers.DTOs.SellerInformation.Document;

namespace DREAMHOMES.Controllers.DTOs.SellerInformation
{
    public class SellerInformationDetailedDTO : SellerInformationDTO
    {
        /// <summary>
        /// Gets or sets the Country.
        /// </summary>
        public string Country { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of Photos/Videos associated to the Listing.
        /// </summary>
        public IList<DocumentLiteDTO> DocumentList { get; set; } = null!;
    }
}
