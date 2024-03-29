using Microsoft.AspNetCore.Identity;

namespace SPOTAHOME.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<IdentityUser> GetUserByToken(string token);
    }
}
