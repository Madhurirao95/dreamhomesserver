using DREAMHOMES.Hubs;
using DREAMHOMES.Models.Repository.Db_Context;
using DREAMHOMES.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static DREAMHOMES.Models.Conversation;

namespace DREAMHOMES.Models.Repository
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly DreamhomesContext _context;

        public ConversationRepository(DreamhomesContext context)
        {
            _context = context;
        }

        public async Task Add(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task<Conversation> Get(string id)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<IEnumerable<Conversation>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatMessage>> GetConversationMessages(string conversationId)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetUserConversationHistory(string userId)
        {
            return await _context.Conversations
                .Where(c => c.UserId == userId)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.StartTime)
                .ToListAsync();
        }

        public async Task Update(Conversation conversation)
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
        }

        public Task Delete(Conversation entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Conversation>> GetActiveConversationsByAgent(string agentId)
        {
            return await _context.Conversations
                .Where(c => c.AgentId == agentId && c.Status == ConversationStatus.Active)
                .ToListAsync();
        }

        public async Task<List<AgentLoad>> GetAgentLoads(List<string> agentIds)
        {
            var loads = await _context.Conversations
                .Where(c => agentIds.Contains(c.AgentId) && c.Status == ConversationStatus.Active)
                .GroupBy(c => c.AgentId)
                .Select(g => new AgentLoad
                {
                    AgentId = g.Key,
                    ActiveChats = g.Count()
                })
                .ToListAsync();

            // Include agents with zero active chats
            foreach (var agentId in agentIds)
            {
                if (!loads.Any(l => l.AgentId == agentId))
                {
                    loads.Add(new AgentLoad { AgentId = agentId, ActiveChats = 0 });
                }
            }

            return loads;
        }

        public async Task<int> GetAgentCurrentLoad(string agentId)
        {
            return await _context.Conversations
            .Where(c => c.AgentId == agentId && c.Status == ConversationStatus.Active)
            .CountAsync();
        }

        public async Task<Conversation> SaveConversation(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task EndConversation(string conversationId)
        {
            var conversation = await _context.Conversations
                .FindAsync(conversationId);

            if (conversation != null)
            {
                conversation.Status = ConversationStatus.Ended;
                conversation.EndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
