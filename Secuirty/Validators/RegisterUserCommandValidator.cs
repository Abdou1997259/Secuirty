using FluentValidation;
using Secuirty.Commands;
using Secuirty.Services;

namespace Secuirty.Validators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IValidationService _validationService;
        public RegisterUserCommandValidator(IValidationService validationService)
        {
            _validationService = validationService;
            Validations();
        }
        public void Validations()
        {
            RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotNull().NotEmpty().WithMessage("must be have value").EmailAddress().WithMessage("Invalid Email ").MustAsync(async (email, _) =>
            await _validationService.UserExistenceByEmail(email)).WithMessage("Already existed");

            RuleFor(x => x.UserName).Cascade(CascadeMode.Stop).NotNull().NotEmpty().WithMessage("must be have value").MustAsync(async (username, _) => await
            _validationService.UserExistenceByUserName(username)).WithMessage("Already existed");
            RuleFor(x => x.Password).Cascade(CascadeMode.Stop).NotNull().NotEmpty().Length(6, 15);
            RuleFor(x => x.FirstName).Cascade(CascadeMode.Stop).NotNull().NotEmpty().Length(6, 15);
            RuleFor(x => x.LastName).Cascade(CascadeMode.Stop).NotNull().NotEmpty().Length(6, 15);
            RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop).NotNull().NotEmpty().Equal(x => x.Password).WithMessage("Ensure the confirm Password");
        }
    }
}
