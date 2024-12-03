
using MediatR;
using Secuirty.Dtos;

namespace Secuirty.Commands
{
    public class RegisterUserCommand : IRequest<Response<AutModel>>
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassowrd { get; set; }
    }
}
