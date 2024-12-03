using MediatR;
using Microsoft.AspNetCore.Mvc;
using Secuirty.Commands;
using Secuirty.Dtos;
using Secuirty.Services;
using System.Threading.Tasks;

namespace Secuirty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly ISender _sender;
        public AuthController(IAuthService service, ISender sender)
        {
            _service = service;
            _sender = sender;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserCommand model)
        {
            var result = await _sender.Send(model);

            return CreateResponse(result);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await _service.LoginAsync(model);

            return CreateResponse(result);
        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenModel model)
        {
            var result = await _service.RefreshTokenAsync(model);

            return CreateResponse(result);
        }
        [HttpPost("RevokeRefreshToken")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] string token)
        {
            var result = await _service.Revoke(token);

            return CreateResponse(result);
        }
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmationModel token)
        {
            var result = await _service.ConfirmEmail(token);

            return CreateResponse(result);
        }
        [HttpPost("ResendConfirmationMessage")]
        public async Task<IActionResult> ResendConfirmationMessage(string email)
        {
            var result = await _service.ResendConfirmationMessage(email);

            return CreateResponse(result);

        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPasswordAsync(string email)
        {
            var result = await _service.ForgetPasswordAsync(email);

            return CreateResponse(result);

        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordModel model)
        {
            var result = await _service.ResetPasswordAsync(model);

            return CreateResponse(result);

        }
        private IActionResult CreateResponse<T>(Response<T> response)
        {
            return StatusCode(response.StatusCode, response);
        }

    }
}
