using FluentValidation;

namespace DREAMHOMES.Models.Rules
{
    public class SellerInformationValidator : AbstractValidator<SellerInformation>
    {
        public SellerInformationValidator() 
        {
            RuleFor(x => x.NumberOfFirePlace).NotNull().NotEmpty().When(y => y.HasFirePlace);
            RuleFor(x => x.NumberOfGarageSpace).NotNull().NotEmpty().When(y => y.HasGarage);
        }
    }
}
