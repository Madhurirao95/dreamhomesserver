using Microsoft.AspNetCore.Identity;

namespace DREAMHOMES.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets if user is an agent.
        /// </summary>
        public bool IsAgent { get; set; }

        /// <summary>
        /// Gets or sets if the user in agent role is online or not.
        /// </summary>
        public bool? IsOnline { get; set; }

        /// <summary>
        /// Gets or sets the Maximum Concurrent Chats of the user in agent role can have.
        /// </summary>
        public int? MaxConcurrentChats { get; set; }

        /// <summary>
        /// Gets or sets the Last Active Time of the user in agent role.
        /// </summary>
        public DateTime? LastActiveTime { get; set; }
    }
}
