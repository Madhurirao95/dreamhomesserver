using DREAMHOMES.Controllers.DTOs.SellerInformation.Document;

namespace DREAMHOMES.Controllers.DTOs.SellerInformation
{
    /// <summary>
    /// Resprests a structure that sends minimal data regarding <see cref="SellerInformation"/>
    /// </summary>
    public class SellerInformationLiteDTO
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Street Address.
        /// </summary>
        public string StreetAddress { get; set; } = null!;

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        public string City { get; set; } = null!;

        /// <summary>
        /// Gets or sets the State.
        /// </summary>
        public string State { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Zip Code.
        /// </summary>
        public string ZipCode { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Country.
        /// </summary>
        public string Country { get; set; } = null!;

        /// <summary>
        /// Gets or sets a random document associated to the Seller.
        /// </summary>
        public DocumentLiteDTO RandomDocument { get; set; } = null!;
    }
}
