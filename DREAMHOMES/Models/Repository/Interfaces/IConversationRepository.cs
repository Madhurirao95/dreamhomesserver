using DREAMHOMES.Hubs;

namespace DREAMHOMES.Models.Repository.Interfaces
{
    public interface IConversationRepository : IDataRepository<Conversation, string>
    {
        Task<List<ChatMessage>> GetConversationMessages(string conversationId);

        Task<List<Conversation>> GetUserConversationHistory(string userId);

        Task<List<Conversation>> GetActiveConversationsByAgent(string agentId);

        Task<List<AgentLoad>> GetAgentLoads(List<string> agentIds);

        Task<int> GetAgentCurrentLoad(string agentId);

        Task<Conversation> SaveConversation(Conversation conversation);

        Task EndConversation(string conversationId);
    }
}
