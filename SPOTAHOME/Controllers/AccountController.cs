using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPOTAHOME.Controllers.DTOs.Account;
using SPOTAHOME.Controllers.DTOs.ValidationResult;
using SPOTAHOME.Models;
using SPOTAHOME.Services.Interfaces;

namespace SPOTAHOME.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;

        private readonly IMapper _mapper;

        private readonly IAccountService _service;

        public AccountController(ILogger<AccountController> logger, IMapper mapper, IAccountService accountService)
        {
            _logger = logger;
            _mapper = mapper;
            _service = accountService;
        }

        [HttpPost("createAccount")]
        public IActionResult CreateAccount([FromBody] AccountPostDTO accountPostDTO)
        {
            if (accountPostDTO is null)
            {
                _logger.LogInformation("Account cannot be empty. Please provide a valid value");
                return BadRequest("Account is null.");
            }

            var errorResult = _service.CreateAccount(_mapper.Map<Account>(accountPostDTO));

            if(errorResult.Any())
            {
                _logger.LogInformation("Account cannot be created due to some errors!");
                return BadRequest(errorResult.Select(x => x.ErrorMessage));
            }

            _logger.LogInformation("Account created successfully!");

            return Ok();
        }


        [HttpPost("signIn")]
        public IActionResult SignIn([FromBody] AccountPostDTO accountPostDTO)
        {
            if (accountPostDTO is null)
            {
                _logger.LogInformation("Account cannot be empty. Please provide a valid value");
                return BadRequest("Account is null.");
            }

            var errorResult = _service.SignIn(_mapper.Map<Account>(accountPostDTO));

            if (errorResult.Any())
            {
                _logger.LogInformation("Account cannot be signed in due to some errors");
                return BadRequest(errorResult.Select(x => x.ErrorMessage));
            }

            _logger.LogInformation("Sign In successful!");

            return Ok();
        }
    }
}