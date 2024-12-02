using Secuirty.Dtos;
using System.Threading.Tasks;

namespace Secuirty.Services
{
    public interface IAuthService
    {
        Task<Response<AutModel>> RegisterAsync(RegisterModel model);
        Task<Response<AutModel>> LoginAsync(LoginModel model);
        Task<Response<AutModel>> RefreshTokenAsync(RefreshTokenModel model);
        Task<Response<string>> Revoke(string token);
        Task<Response<string>> ConfirmEmail(ConfirmationModel model);
        Task<Response<string>> ResendConfirmationMessage(string email);
    }
}
