using DREAMHOMES.Controllers.DTOs.SellerInformation.Document;

namespace DREAMHOMES.Controllers.DTOs.SellerInformation
{
    /// <summary>
    /// Resprests a structure that sends data regarding <see cref="SellerInformation"/>
    /// </summary>
    public class SellerInformationDetailedGetDTO : SellerInformationGetDTO
    {
        /// <summary>
        /// Gets or sets the <see cref="SellerInformation.TypeOfListing"/>
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Listing Price.
        /// </summary>
        public double ListingPrice { get; set; }

        /// <summary>
        /// Gets or sets the Area.
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Gets or sets the X Co-ordinate.
        /// </summary>
        public double CoordinateX { get; set; }

        /// <summary>
        /// Gets or sets the Y Co-ordinate.
        /// </summary>
        public double CoordinateY { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Properties.
        /// </summary>
        public string Properties { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of document associated to the Seller.
        /// </summary>
        public ICollection<DocumentLiteDTO> DocumentList { get; set; }
    }
}
