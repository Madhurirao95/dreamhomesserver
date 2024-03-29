namespace DREAMHOMES.Models
{
    /// <summary>
    /// Represent the model for any type of Document to be stored in the system.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Document.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the Date of Creation of the Document.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the Date of Modification for the Document.
        /// </summary>
        public DateTime DateModified { get; set; }

        /// <summary>
        /// Gets or sets the Extension of the Document.
        /// </summary>
        public string? Extension { get; set; }

        /// <summary>
        /// Gets or sets the Size of the Document.
        /// </summary>
        public double? Size { get; set; }

        /// <summary>
        /// Gets or sets the File Path of the Document.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Gets or sets the File Type.
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// Gets or sets the Author Name.
        /// </summary>
        public string? AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the <see cref="SellerInformation"/>.
        /// </summary>
        public int SellerId { get; set;}
    }
}
