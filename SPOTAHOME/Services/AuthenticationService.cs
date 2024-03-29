
using Microsoft.AspNetCore.Identity;
using SPOTAHOME.Services.Interfaces;

namespace SPOTAHOME.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        Task<IdentityUser> IAuthenticationService.GetUserByToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
