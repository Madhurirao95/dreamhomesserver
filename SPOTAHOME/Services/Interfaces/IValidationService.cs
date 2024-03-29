using System.ComponentModel.DataAnnotations;

namespace SPOTAHOME.Services.Interfaces
{
    /// <summary>
    /// Represent the methods given by validation service.
    /// Follows "Strategy Pattern" partially.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Adds the validation methods to the list.
        /// </summary>
        /// <param name="validationMethod"></param>
        void Add(Func<Task<IEnumerable<ValidationResult>>> validationMethod);

        /// <summary>
        /// Executes all the validations.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ValidationResult>> ValidateAll();
    }
}
