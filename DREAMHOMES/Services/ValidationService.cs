using DREAMHOMES.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace DREAMHOMES.Services
{
    public class ValidationService : IValidationService
    {
        private List<Func<Task<IEnumerable<ValidationResult>>>> validationMethods = new List<Func<Task<IEnumerable<ValidationResult>>>>();

        public void Add(Func<Task<IEnumerable<ValidationResult>>> validationMethod)
        {
            validationMethods.Add(validationMethod);
        }

        public async Task<IEnumerable<ValidationResult>> ValidateAll()
        {
            var results = new List<ValidationResult>();
            foreach (var validationMethod in validationMethods)
            {
                results.AddRange(await validationMethod.Invoke());
            }
            return results;
        }
    }
}
