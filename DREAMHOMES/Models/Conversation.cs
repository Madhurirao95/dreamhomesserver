using System.ComponentModel.DataAnnotations;
using static DREAMHOMES.Models.SellerInformation;

namespace DREAMHOMES.Models
{
    public class Conversation
    {
        /// <summary>
        /// Represents the ENUM for Status of Conversation.
        /// </summary>
        public enum ConversationStatus
        {
            Abandoned,
            Active,
            Ended
        }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the User ID.
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the User.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Agent ID.
        /// </summary>
        [Required]
        public string AgentId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Agent.
        /// </summary>
        public ApplicationUser Agent { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Start time.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the End Time.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public ConversationStatus Status { get; set; } = ConversationStatus.Active;

        // Navigation properties
        public IList<ChatMessage> Messages { get; set; } = null!;
    }
}
