using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

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
        public enum TypeOfListing
        {
            None,
            House,
            Apartment,
            Plot
        }

        /// <summary>
        /// Gets of sets the ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets of sets the Street Address.
        /// </summary>
        public string StreetAddress { get; set; } = null!;

        /// <summary>
        /// Gets of sets the City.
        /// </summary>
        public string City { get; set; } = null!;

        /// <summary>
        /// Gets of sets the Unit.
        /// </summary>
        [JsonIgnore]
        public string? Unit { get; set; }

        /// <summary>
        /// Gets of sets the Zip Code.
        /// </summary>
        public string ZipCode { get; set; } = null!;

        /// <summary>
        /// Gets of sets the Country Code.
        /// </summary>
        public string CountryCode { get; set; } = "US";

        /// <summary>
        /// Gets of sets the State.
        /// </summary>
        public States State { get; set; } = States.None;

        /// <summary>
        /// Gets of sets the Contact Number.
        /// </summary>
        [JsonIgnore]
        public string? ContactNumber { get; set; }

        /// <summary>
        /// Gets of sets the Remarks.
        /// </summary>
        [JsonIgnore]
        public string? Remarks { get; set; }

        /// <summary>
        /// Gets of sets the Price.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Gets of sets the Area.
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Gets of sets the Type of Listing.
        /// </summary>
        public TypeOfListing Type { get; set; } = TypeOfListing.None;

        /// <summary>
        /// Gets of sets the list of <see cref="Document"/> .
        /// </summary>
        public IList<Document> Documents { get; set; } = null!;

        //Entity Relationships
        /// <summary>
        /// Gets or sets the string User ID.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Seller itself.
        /// </summary>
        public IdentityUser User { get; set; } = null!;
    }
}
