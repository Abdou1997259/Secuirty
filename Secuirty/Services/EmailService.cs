using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Secuirty.Dtos;

using System.Threading.Tasks;

namespace Secuirty.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;

        }
        public async Task SendAsync(EmailModel model)
        {
            MimeMessage mimeMessage = new MimeMessage();
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = model.Body;
            bodyBuilder.HtmlBody = model.Body;
            mimeMessage.Subject = model.Subject;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            foreach (var account in model.To)
                mimeMessage.To.Add(new MailboxAddress("", account));

            mimeMessage.From.Add(new MailboxAddress("Manger", _settings.GamailAccount));
            using (var smpt = new SmtpClient())
            {
                await smpt.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                await smpt.AuthenticateAsync(_settings.GamailAccount, _settings.AppPassword);
                await smpt.SendAsync(mimeMessage);
                await smpt.DisconnectAsync(true);
            }


        }
    }
}
