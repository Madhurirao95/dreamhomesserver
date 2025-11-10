using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace DREAMHOMES.Models
{
    /// <summary>
    /// Reprsents the model for the Seller's Information Details.
    /// </summary>
    public class SellerInformation
    {
        /// <summary>
        /// Represents an ENUM for the 50 states in the US.
        /// </summary>
        public enum States
        {
            None,
            AK,
            AL,
            AZ,
            AR,
            CA,
            CO,
            CT,
            DE,
            FL,
            GA,
            HI,
            ID,
            IL,
            IN,
            IA,
            KS,
            KY,
            LA,
            ME,
            MD,
            MA,
            MI,
            MN,
            MS,
            MO,
            MT,
            NE,
            NV,
            NH,
            NJ,
            NM,
            NY,
            NC,
            ND,
            OH,
            OK,
            OR,
            PA,
            RI,
            SC,
            SD,
            TN,
            TX,
            UT,
            VT,
            VA,
            WA,
            WV,
            WI,
            WY
        }

        /// <summary>
        /// Represents the ENUM for type of Listing.
        /// </summary>
        public enum ListingType
        {
            None,
            House,
            TownHouse,
            Condominium,
            Land
        }

        /// <summary>
        /// Represents the ENUM for type of Building.
        /// </summary>
        public enum TypeOfBuilding
        {
            None,
            Resale,
            NewConstruction
        }

        /// <summary>
        /// Represents the ENUM for status of Listing.
        /// </summary>
        public enum ListingStatus
        {
            OffMarket,
            Active,
            ComingSoon,
            UnderContractOrPending,
            Sold
        }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Street Address.
        /// </summary>
        [MaxLength(1000)]
        public string StreetAddress { get; set; } = null!;

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        [MaxLength(100)]
        public string City { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Unit.
        /// </summary>
        [MaxLength(50)]
        public string? Unit { get; set; }

        /// <summary>
        /// Gets or sets the Zip Code.
        /// </summary>
        [MaxLength(50)]
        public string ZipCode { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Country Code.
        /// </summary>
        [MaxLength(10)]
        public string CountryCode { get; set; } = "US";

        /// <summary>
        /// Gets or sets the State.
        /// </summary>  
        public States State { get; set; } = States.None;

        /// <summary>
        /// Gets or sets the Contact Number.
        /// </summary>
        [MaxLength(10)]
        public string? ContactNumber { get; set; }

        /// <summary>
        /// Gets or sets the Listing Price.
        /// </summary>
        public double ListingPrice { get; set; }

        /// <summary>
        /// Gets or sets the Sold Price.
        /// </summary>
        public double SoldPrice { get; set; }

        /// <summary>
        /// Gets or sets the Area.
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Gets or sets the Latitude and Longitude of the Address.
        /// </summary>
        public Point Location { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Type of Listing.
        /// </summary>
        public ListingType Type { get; set; } = ListingType.None;

        /// <summary>
        /// Gets or sets the Status of Listing.
        /// </summary>
        public ListingStatus Status { get; set; } = ListingStatus.Active;

        /// <summary>
        /// Gets or sets the Type of Building.
        /// </summary>
        public TypeOfBuilding BuildingType { get; set; } = TypeOfBuilding.Resale;

        /// <summary>
        /// Gets or sets the Year Built.
        /// </summary>
        public int YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets the Lot Area.
        /// </summary>
        public double LotArea { get; set; }

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
        public string? Properties { get; set; }

        /// <summary>
        /// Gets or sets the description of the Listing.
        /// </summary>
        public string? Description { get; set; }

        //Entity Relationships

        /// <summary>
        /// Gets or sets the list of <see cref="Document"/> .
        /// </summary>
        public IList<Document> Documents { get; set; } = null!;
        /// <summary>
        /// Gets or sets the string User ID.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Seller itself.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;
    }
}
