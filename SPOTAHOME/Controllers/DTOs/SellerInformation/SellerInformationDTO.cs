namespace SPOTAHOME.Controllers.DTOs.SellerInformation
{
    public class SellerInformationDTO
    {
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
        /// Gets or sets a Contact Number.
        /// </summary>
        public string ContactNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Remarks.
        /// </summary>
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Unit number.
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="SellerInformation.TypeOfListing"/>
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Price.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Gets or sets the Area.
        /// </summary>
        public double Area { get; set; }
    }
}
