using static DREAMHOMES.Models.SellerInformation;

namespace DREAMHOMES.Controllers.DTOs.SellerInformation
{
    public class SellerInformationDTO
    {
        /// <summary>
        /// Gets or sets the Street Address.
        /// </summary>
        public string StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the State.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the Zip Code.
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets a Contact Number.
        /// </summary>
        public string ContactNumber { get; set; }

        /// <summary>
        /// Gets or sets the Unit number.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SellerInformation.TypeOfListing"/>
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Price.
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
        /// Gets or sets the Status of Listing.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the Type of Building.
        /// </summary>
        public string BuildingType { get; set; }

        /// <summary>
        /// Gets or sets the Year Built.
        /// </summary>
        public int YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets the Lot Area.
        /// </summary>
        public double LotArea { get; set; }

        /// <summary>
        /// Gets or sets the Lot Area Unit.
        /// </summary>
        public string LotAreaUnit { get; set; }

        /// <summary>
        /// Gets or sets the HOA Amount.
        /// </summary>
        public double HOA { get; set; }

        /// <summary>
        /// Gets or sets the Number of BedRooms.
        /// </summary>
        public double BedRooms { get; set; }

        /// <summary>
        /// Gets or sets the Number of BathRooms.
        /// </summary>
        public double BathRooms { get; set; }

        /// <summary>
        /// Gets or sets the Amount Per Sq. Ft.
        /// </summary>
        public double AmountPerSqFt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Fire Place is present or not.
        /// </summary>
        public bool HasFirePlace { get; set; }

        /// <summary>
        /// Gets or sets the Number of Fire Places.
        /// </summary>
        public int NumberOfFirePlace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Garage Space is present or not.
        /// </summary>
        public bool HasGarage { get; set; }

        /// <summary>
        /// Gets or sets the Number of Garage Space.
        /// </summary>
        public int NumberOfGarageSpace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Pool is present or not.
        /// </summary>
        public bool HasPool { get; set; }

        /// <summary>
        /// Gets or sets the list of key value pairs as property.
        /// </summary>
        public string Properties { get; set; }

        /// <summary>
        /// Gets or sets the Description of the Listing.
        /// </summary>
        public string Description { get; set; }
    }
}
