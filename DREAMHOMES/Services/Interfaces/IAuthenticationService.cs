using Microsoft.AspNetCore.Identity;

namespace DREAMHOMES.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<IdentityUser> GetUserByToken(string token);
    }
}
