using SPOTAHOME.Models;
using System.ComponentModel.DataAnnotations;

namespace SPOTAHOME.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<ValidationResult> CreateAccount(Account account);

        IEnumerable<ValidationResult> SignIn(Account account);
    }
}
