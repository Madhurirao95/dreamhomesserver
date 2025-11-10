using AutoMapper;
using DREAMHOMES.Models;
using DREAMHOMES.Models.Repository.Interfaces;
using DREAMHOMES.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DREAMHOMES.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task UpdateUserStatus(string userId, bool isOnline)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return;

            user.IsOnline = isOnline;

            await _userManager.UpdateAsync(user);
        }

        public async Task<int?> GetAgentMaxChats(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.IsAgent == true) 
                return user.MaxConcurrentChats;
            return 0;
        }

        public async Task<string> GetUserEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email ?? "Unknown";
        }

        public async Task<string> GetUserName(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.UserName ?? user?.Email ?? "Unknown";
        }

        public async Task<bool> IsAgentOnline(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.IsAgent == true && user.IsOnline == true)
                return true;
            return false;
        }
    }
}
