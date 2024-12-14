using FluentValidation;
using MediatR;
using Secuirty.Commands;
using Secuirty.Dtos;
using Secuirty.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Secuirty.Handlers
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Response<AuthModel>>
    {

        private readonly IAuthService _authService;
        public RegisterUserHandler(IValidator<RegisterUserCommand> validator, IAuthService authService)
        {

            _authService = authService;
        }
        public async Task<Response<AuthModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {

            return await _authService.RegisterAsync(request);


        }
    }
}
