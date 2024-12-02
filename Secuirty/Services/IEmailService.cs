using Secuirty.Dtos;
using System.Threading.Tasks;

namespace Secuirty.Services
{
    public interface IEmailService
    {
        Task SendAsync(EmailModel model);
    }
}
