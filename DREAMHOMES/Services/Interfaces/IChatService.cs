using DREAMHOMES.Hubs;
using DREAMHOMES.Models;

namespace DREAMHOMES.Services.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatMessage>> GetConversationMessages(string conversationId);

        Task<List<Conversation>> GetUserConversationHistory(string userId);

        Task<ChatMessage> CreateMessage(string userId, string conversationId, string message, DateTime timeStamp, bool isFromAgent);

        Task<Conversation> CreateConversation(string userId, string agentId);

        Task<Conversation> GetConversation(string conversationId);

        Task<List<Conversation>> GetActiveConversationsByAgent(string agentId);

        Task<List<AgentLoad>> GetAgentLoads(List<string> agentIds);

        Task<int> GetAgentCurrentLoad(string agentId);

        Task EndConversation(string conversationId);
    }
}
