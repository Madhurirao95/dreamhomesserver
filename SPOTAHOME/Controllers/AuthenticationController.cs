using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SPOTAHOME.Configuration;
using SPOTAHOME.Controllers.DTOs.Account;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;

namespace SPOTAHOME.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly JwtBearerTokenSettings _jwtBearerTokenSettings = null!;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthenticationController(IOptions<JwtBearerTokenSettings> jwtTokenOptions, ILogger<AuthenticationController> logger, UserManager<IdentityUser> userManager)
        {
            _jwtBearerTokenSettings = jwtTokenOptions.Value;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost("createAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountPostDTO accountPostDTO)
        {
            if (accountPostDTO == null)
            {
                return new BadRequestObjectResult(new { Message = "Account Creation Failed" });
            }

            var identityUser = new IdentityUser() { UserName = accountPostDTO.Email, Email = accountPostDTO.Email };
            var creationResult = await _userManager.CreateAsync(identityUser, accountPostDTO.Password);
            if (!creationResult.Succeeded)
            {
                var errorsDictionary = new Dictionary<string, string>();
                foreach (IdentityError error in creationResult.Errors)
                {
                    errorsDictionary.Add(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "Account Creation Failed", Errors = errorsDictionary });
            }

            _logger.LogInformation("Account created successfully!");

            return Ok(new { Message = "Account Creation Successful" });
        }


        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn([FromBody] AccountPostDTO accountPostDTO)
        {

            IdentityUser? identityUser;

            if (accountPostDTO == null
                || (identityUser = await ValidateUser(accountPostDTO)) == null)
            {
                return new BadRequestObjectResult(new { Message = "Sign In failed" });
            }

            var token = GenerateToken(identityUser);

            _logger.LogInformation("Sign In successful!");

            return Ok(new { Token = token, Message = "Success" });
        }

        private async Task<IdentityUser?> ValidateUser(AccountPostDTO accountPostDTO)
        {
            var identityUser = await _userManager.FindByEmailAsync(accountPostDTO.Email);
            if (identityUser != null)
            {
                var result = _userManager.PasswordHasher.VerifyHashedPassword(identityUser, identityUser.PasswordHash, accountPostDTO.Password);
                return result == PasswordVerificationResult.Failed ? null : identityUser;
            }

            return null;
        }

        private object GenerateToken(IdentityUser identityUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtBearerTokenSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, identityUser.Email)
                }),

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