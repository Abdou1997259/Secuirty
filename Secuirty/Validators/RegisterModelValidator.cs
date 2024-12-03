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
            RuleFor(x => x.Email)
             .Cascade(CascadeMode.Stop)
             .NotNull().WithMessage("{PropertyName} must have a value")
             .NotEmpty().WithMessage("{PropertyName} must have a value")
             .EmailAddress().WithMessage("Invalid {PropertyName}")
             .MustAsync(async (email, _) => await _validationService.UserExistenceByEmail(email))
             .WithMessage("{PropertyName} is already existed");

            RuleFor(x => x.UserName)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("{PropertyName} must have a value")
                .NotEmpty().WithMessage("{PropertyName} must have a value")
                .MustAsync(async (username, _) => await _validationService.UserExistenceByUserName(username))
                .WithMessage("{PropertyName} is already existed");



        }

    }
}
