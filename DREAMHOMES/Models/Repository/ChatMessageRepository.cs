using DREAMHOMES.Models.Repository.Db_Context;
using DREAMHOMES.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DREAMHOMES.Models.Repository
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly DreamhomesContext _context;

        public ChatMessageRepository(DreamhomesContext context)
        {
            _context = context;
        }

        public async Task Add(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public Task Delete(ChatMessage entity)
        {
            throw new NotImplementedException();
        }

        public Task<ChatMessage> Get(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChatMessage>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<ChatMessage> SaveMessage(ChatMessage chatMessage)
        {
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return chatMessage;
        }

        public Task Update(ChatMessage entityToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
