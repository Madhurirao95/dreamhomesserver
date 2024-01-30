using SPOTAHOME.Models;
using SPOTAHOME.Models.Repository.Interfaces;
using SPOTAHOME.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SPOTAHOME.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        public AccountService(IAccountRepository accountRepository) 
        { 
            _accountRepository = accountRepository;
        }

        public IEnumerable<ValidationResult> CreateAccount(Account account)
        {
            var results = new List<ValidationResult>();

            var existingAccount = _accountRepository.GetByEmail(account.Email);

            if (existingAccount == null)
            {
                _accountRepository.Add(account);
            } else
            {
                results.Add(new ValidationResult("Account with same Email already exists. Please Sign In."));
            }

            return results;
        }

        public IEnumerable<ValidationResult> SignIn(Account account)
        {
            var results = new List<ValidationResult>();

            var existingAccount = _accountRepository.GetByEmail(account.Email);

            if(existingAccount == null)
            {
                results.Add(new ValidationResult("Email doesn't exists. Please Create an Account."));
            }

            return results;
        }

    }
}
