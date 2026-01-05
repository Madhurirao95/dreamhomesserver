using DREAMHOMES.Models;
using Microsoft.AspNetCore.Identity;

namespace DREAMHOMES.Services.Interfaces
{
    public interface IUserService
    {
        Task UpdateUserStatus(string userId, bool isOnline);

        Task<string> GetUserName(string userId);

        Task<string> GetUserEmail(string userId);

        Task<int?> GetAgentMaxChats(string agentId);

        Task<bool> IsAgentOnline(string userId);

        Task<ApplicationUser> GetUserByEmail(string email);

        Task<IdentityResult> CreateUserWithRole(string email, string password, bool isAgent = false);

        Task<IdentityResult> ResetPassword(string email, string password);
    }
}
