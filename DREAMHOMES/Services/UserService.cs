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

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await this._userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUserWithRole(string email, string password, bool isAgent)
        {
            var applicationUser = new ApplicationUser() { UserName = email, Email = email };
            if (isAgent)
            {
                applicationUser.IsAgent = true;
                applicationUser.MaxConcurrentChats = 50; // Default value for agents
            }
            var creationResult = await _userManager.CreateAsync(applicationUser, password);

            if (creationResult.Succeeded)
            {
                if (isAgent)
                    await _userManager.AddToRoleAsync(applicationUser, "Agent");
                else
                    await _userManager.AddToRoleAsync(applicationUser, "User");
            }

            return creationResult;
        }

        public async Task<IdentityResult> ResetPassword(string email, string password)
        {
            var user = await this.GetUserByEmail(email);
            if (user == null) return 
                    IdentityResult.Failed(new IdentityError { Description = "User not found." });

            // Directly reset password (not recommended for production)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, password);

        }
    }
}
