using Secuirty.Commands;
using Secuirty.Dtos;
using System.Threading.Tasks;

namespace Secuirty.Services
{
    public interface IAuthService
    {
        Task<Response<AuthModel>> RegisterAsync(RegisterUserCommand model);
        Task<Response<AuthModel>> RegsiterWithThirdParty(RegisterWithThirdPartyModel model);
        Task<Response<AuthModel>> LoginAsync(LoginModel model);
        Task<Response<AuthModel>> LoginWithThirdPartyAsync(LoginWithThirdPartyModel model);
        Task<Response<AuthModel>> RefreshTokenAsync(RefreshTokenModel model);
        Task<Response<string>> Revoke(string token);
        Task<Response<string>> ConfirmEmail(ConfirmationModel model);
        Task<Response<string>> ResendConfirmationMessage(string email);
        Task<Response<string>> ForgetPasswordAsync(string email);
        Task<Response<string>> ResetPasswordAsync(ResetPasswordModel model);
        Task<Response<AuthModel>> GoogleLoginAsync(string idToken);

    }
}
