using Secuirty.Dtos;
using System.Threading.Tasks;

namespace Secuirty.Services
{
    public interface IValidationService
    {
        Task<bool> UserExistenceByUserName(string userName);
        Task<bool> UserExistenceByEmail(string email);
        Task<bool> TokenValidating(RefreshTokenModel token);

    }
}
