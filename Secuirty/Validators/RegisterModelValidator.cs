using FluentValidation;
using Secuirty.Dtos;
using Secuirty.Services;

namespace Secuirty.Validators
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        private readonly IValidationService _validationService;
        public RegisterModelValidator(IValidationService validationService)
        {
            _validationService = validationService;
            Validations();
        }
        public void Validations()
        {
            RuleFor(x => x.Email).MustAsync(async (email, _) =>
            await _validationService.UserExistenceByEmail(email)).WithMessage("Already existed");

            RuleFor(x => x.UserName).MustAsync(async (username, _) => await
            _validationService.UserExistenceByUserName(username)).WithMessage("Already existed");


        }

    }
}
