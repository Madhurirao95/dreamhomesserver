namespace DREAMHOMES.Models.Repository.Interfaces
{
    public interface IChatMessageRepository: IDataRepository<ChatMessage, string>
    {
        Task<ChatMessage> SaveMessage(ChatMessage chatMessage);
    }
}
