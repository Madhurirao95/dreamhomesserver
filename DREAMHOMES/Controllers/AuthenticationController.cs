using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DREAMHOMES.Configuration;
using DREAMHOMES.Controllers.DTOs.Account;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;
using DREAMHOMES.Models;

namespace DREAMHOMES.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly JwtBearerTokenSettings _jwtBearerTokenSettings = null!;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticationController(IOptions<JwtBearerTokenSettings> jwtTokenOptions, ILogger<AuthenticationController> logger, UserManager<ApplicationUser> userManager)
        {
            _jwtBearerTokenSettings = jwtTokenOptions.Value;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost("createAccount/user")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountPostDTO accountPostDTO)
        {
            if (accountPostDTO == null)
            {
                return new BadRequestObjectResult(new { Message = "Account Creation Failed" });
            }

            var applicationUser = new ApplicationUser() { UserName = accountPostDTO.Email, Email = accountPostDTO.Email };
            var creationResult = await _userManager.CreateAsync(applicationUser, accountPostDTO.Password);
            if (!creationResult.Succeeded)
            {
                var errorsDictionary = new Dictionary<string, string>();
                foreach (IdentityError error in creationResult.Errors)
                {
                    errorsDictionary.Add(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "Account Creation Failed", Errors = errorsDictionary });
            }
            await _userManager.AddToRoleAsync(applicationUser, "User");
            _logger.LogInformation("Account created successfully for the User!");

            return Ok(new { Message = "Account Creation Successful for User" });
        }

        [HttpPost("createAccount/agent")]
        public async Task<IActionResult> CreateAccountForAgent([FromBody] AccountPostDTO accountPostDTO)
        {
            if (accountPostDTO == null)
            {
                return new BadRequestObjectResult(new { Message = "Account Creation Failed" });
            }

            var applicationUser = new ApplicationUser() { UserName = accountPostDTO.Email, Email = accountPostDTO.Email, IsAgent = true, MaxConcurrentChats = 5 };
            var creationResult = await _userManager.CreateAsync(applicationUser, accountPostDTO.Password);
            if (!creationResult.Succeeded)
            {
                var errorsDictionary = new Dictionary<string, string>();
                foreach (IdentityError error in creationResult.Errors)
                {
                    errorsDictionary.Add(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "Account Creation Failed", Errors = errorsDictionary });
            }
            await _userManager.AddToRoleAsync(applicationUser, "Agent");
            _logger.LogInformation("Account created successfully for the Agent!");

            return Ok(new { Message = "Account Creation Successful for the Agent" });
        }



        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn([FromBody] AccountPostDTO accountPostDTO)
        {

            ApplicationUser applicationUser;

            if (accountPostDTO == null
                || (applicationUser = await ValidateUser(accountPostDTO)) == null)
            {
                return new BadRequestObjectResult(new { Message = "Sign In failed" });
            }

            var token = GenerateToken(applicationUser);

            _logger.LogInformation("Sign In successful!");

            return Ok(new { Token = token, Message = "Success" });
        }

        private async Task<ApplicationUser> ValidateUser(AccountPostDTO accountPostDTO)
        {
            var applicationUser = await _userManager.FindByEmailAsync(accountPostDTO.Email);
            if (applicationUser != null)
            {
                var result = _userManager.PasswordHasher.VerifyHashedPassword(applicationUser, applicationUser.PasswordHash, accountPostDTO.Password);
                return result == PasswordVerificationResult.Failed ? null : applicationUser;
            }

            return null;
        }

        private async Task<object> GenerateToken(ApplicationUser applicationUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtBearerTokenSettings.SecretKey);

            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            var claims = new List<Claim>
            {
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