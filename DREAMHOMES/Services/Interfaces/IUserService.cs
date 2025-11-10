namespace DREAMHOMES.Services.Interfaces
{
    public interface IUserService
    {
        Task UpdateUserStatus(string userId, bool isOnline);

        Task<string> GetUserName(string userId);

        Task<string> GetUserEmail(string userId);

        Task<int?> GetAgentMaxChats(string agentId);

        Task<bool> IsAgentOnline(string userId);
    }
}
