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
using Microsoft.AspNetCore.Authorization;
using DREAMHOMES.Services.Interfaces;

namespace DREAMHOMES.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;

        public AuthenticationController(ILogger<AuthenticationController> logger, IAuthenticationService authenticationService, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
            _authenticationService = authenticationService;
        }

        [HttpPost("createAccount/user")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountPostDTO accountPostDTO)
        {
            if (accountPostDTO == null)
            {
                return new BadRequestObjectResult(new { Message = "Account Creation Failed" });
            }

            var creationResult = await this._authenticationService.CreateAccount(accountPostDTO.Email, accountPostDTO.Password);

            if (!creationResult.Succeeded)
            {
                var errorsDictionary = new Dictionary<string, string>();
                foreach (IdentityError error in creationResult.Errors)
                {
                    errorsDictionary.Add(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "Account Creation Failed", Errors = errorsDictionary });
            }
            
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

            var creationResult = await this._authenticationService.CreateAccount(accountPostDTO.Email, accountPostDTO.Password, true);
            if (!creationResult.Succeeded)
            {
                var errorsDictionary = new Dictionary<string, string>();
                foreach (IdentityError error in creationResult.Errors)
                {
                    errorsDictionary.Add(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "Account Creation Failed", Errors = errorsDictionary });
            }
            _logger.LogInformation("Account created successfully for the Agent!");

            return Ok(new { Message = "Account Creation Successful for the Agent" });
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn([FromBody] AccountPostDTO accountPostDTO)
        {

            if (accountPostDTO == null
                || (await this._userService.GetUserByEmail(accountPostDTO.Email) == null))
            {
                return new BadRequestObjectResult(new { Message = "Your email does not exist. Please Create a new Account!" });
            }

            if(!await this._authenticationService.IsPasswordValid(accountPostDTO.Email, accountPostDTO.Password))
            {
                return new BadRequestObjectResult(new { Message = "The password is incorrect. Please try again!" });
            }
            var token = await this._authenticationService.SignIn(accountPostDTO.Email, accountPostDTO.Password);

            _logger.LogInformation("Sign In successful!");

            return Ok(new { Token = token, Message = "Success" });
        }

        [HttpGet("isAgent")]
        public async Task<IActionResult> IsAgent(string email)
        {
            if (String.IsNullOrEmpty(email))
                return new BadRequestObjectResult(new { Message = "Email is invalid" });

            var applicationUser = await this._userService.GetUserByEmail(email);
            if (applicationUser == null)
                return Ok(false);

            return Ok(applicationUser.IsAgent);
        }

        [HttpGet("isExistingUser")]
        public async Task<IActionResult> IsExistingUser(string email)
        {
            var user = await this._userService.GetUserByEmail(email);
            return Ok(user != null);
        }


        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] AccountPostDTO dto)
        {
            var result = await this._userService.ResetPassword(dto.Email, dto.Password);

            if (result.Succeeded) return Ok();
            return BadRequest(result.Errors);
        }

    }
}