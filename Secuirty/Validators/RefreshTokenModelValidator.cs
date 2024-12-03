using FluentValidation;
using Secuirty.Dtos;
using Secuirty.Services;

namespace Secuirty.Validators
{
    public class RefreshTokenModelValidator : AbstractValidator<RefreshTokenModel>
    {
        private readonly IValidationService _validationService;
        public RefreshTokenModelValidator(IValidationService validationService)
        {
            _validationService = validationService;
            Validations();

        }
        public void Validations()
        {
            RuleFor(x => x).MustAsync(async (tokenModel, _) => await _validationService.TokenValidating(tokenModel));
        }


    }
}
