
using Microsoft.AspNetCore.Identity;
using DREAMHOMES.Services.Interfaces;

namespace DREAMHOMES.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        Task<IdentityUser> IAuthenticationService.GetUserByToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
