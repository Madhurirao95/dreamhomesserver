
using DREAMHOMES.Configuration;
using DREAMHOMES.Controllers.DTOs.Account;
using DREAMHOMES.Models;
using DREAMHOMES.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DREAMHOMES.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly JwtBearerTokenSettings _jwtBearerTokenSettings = null!;

        public AuthenticationService(UserManager<ApplicationUser> userManager,IUserService userService, IOptions<JwtBearerTokenSettings> jwtTokenOptions)
        {
            _userService = userService;
            _userManager = userManager;
            _jwtBearerTokenSettings = jwtTokenOptions.Value;
        }

        public async Task<IdentityResult> CreateAccount(string email, string password, bool isAgent = false)
        {
            return await this._userService.CreateUserWithRole(email, password, isAgent);
        }

        public Task<bool> IsAgent(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsAnExistingUser(string email)
        {
            return await this._userService.GetUserByEmail(email) != null ? true: false;
        }


        public async Task<object> SignIn(string email, string password)
        {
            return await this.GenerateToken(await this._userService.GetUserByEmail(email));
        }

        public async Task<bool> IsPasswordValid(string email, string password)
        {
            var applicationUser = await this._userService.GetUserByEmail(email);
            if (applicationUser != null)
            {
                var result = _userManager.PasswordHasher.VerifyHashedPassword(applicationUser, applicationUser.PasswordHash, password);
                return result != PasswordVerificationResult.Failed;
            }

            return false;
        }

        private async Task<object> GenerateToken(ApplicationUser applicationUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtBearerTokenSettings.SecretKey);

            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            var claims = new List<Claim>
            {
            new(ClaimTypes.NameIdentifier, applicationUser.Id),
            new(ClaimTypes.Email, applicationUser.Email)
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                Expires = DateTime.UtcNow.AddMinutes(_jwtBearerTokenSettings.ExpiryTimeInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _jwtBearerTokenSettings.Audience,
                Issuer = _jwtBearerTokenSettings.Issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
