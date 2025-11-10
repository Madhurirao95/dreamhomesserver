using AutoMapper;
using DREAMHOMES.Hubs;
using DREAMHOMES.Models;
using DREAMHOMES.Models.Repository.Interfaces;
using DREAMHOMES.Services.Interfaces;

namespace DREAMHOMES.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        public ChatService(IChatMessageRepository messageRepository, IConversationRepository conversationRepository)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
        }
        public async Task<Conversation> CreateConversation(string userId, string agentId)
        {
            return await this._conversationRepository.SaveConversation(new Conversation { AgentId = agentId, UserId = userId });
        }

        public async Task<ChatMessage> CreateMessage(string userId, string conversationId, string message, DateTime timeStamp, bool isFromAgent)
        {
            return await this._messageRepository.SaveMessage(new ChatMessage { Content = message, ConversationId = conversationId, UserId = userId, IsFromAgent = isFromAgent, Timestamp = timeStamp });
        }

        public async Task EndConversation(string conversationId)
        {
            await this._conversationRepository.EndConversation(conversationId);
        }

        public async Task<List<Conversation>> GetActiveConversationsByAgent(string agentId)
        {
            return await this._conversationRepository.GetActiveConversationsByAgent(agentId);
        }

        public async Task<int> GetAgentCurrentLoad(string agentId)
        {
            return await this._conversationRepository.GetAgentCurrentLoad(agentId);
        }

        public async Task<List<AgentLoad>> GetAgentLoads(List<string> agentIds)
        {
            return await this._conversationRepository.GetAgentLoads(agentIds);
        }

        public async Task<Conversation> GetConversation(string conversationId)
        {
            return await this._conversationRepository.Get(conversationId);
        }

        public async Task<List<ChatMessage>> GetConversationMessages(string conversationId)
        {
            return await this._conversationRepository.GetConversationMessages(conversationId);
        }

        public async Task<List<Conversation>> GetUserConversationHistory(string userId)
        {
            return await this._conversationRepository.GetUserConversationHistory(userId);
        }
    }
}
