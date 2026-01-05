using DREAMHOMES.Controllers.DTOs.Account;
using DREAMHOMES.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DREAMHOMES.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<IdentityResult> CreateAccount(string email, string password, bool isAgent = false);

        Task<object> SignIn(string email, string password);

        Task<bool> IsPasswordValid(string email, string password);

        Task<bool> IsAgent(string email);

        Task<bool> IsAnExistingUser(string email);
    }
}
