
namespace DREAMHOMES.Controllers.DTOs.ChatHub
{
    public class ChatMessageDTO
    {
        public string ConversationId { get; set; }

        public string UserId { get; set; } = null!;

        public string Content { get; set; }

        public bool IsFromAgent { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
