using Application.Interfaces;
using Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MailService : IMailService
    {
        private static readonly string VERIFY_EMAIL_SUBJECT = "Balance Tracker - weryfikacja adresu e-mail";
        private static readonly string VERIFY_EMAIL_BODY = "Witaj {username}!<br/><br/>" +
            "Dziękujemy za rejestrację w serwisie Balance Tracker. " +
            "Aby móc w pełni skorzystać ze wszystkich funkcjonalności, prosimy o weryfikację adresu e-mail. " +
            "W tym celu, kliknij w <a href=\"{url}\">ten link</a>, lub wprowadź poniższy kod:<br/>" +
            "<b>{code}</b>";

        private readonly string fromMail;
        private readonly string displayName;
        private readonly string password;
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string frontendUrl;

        public MailService(IConfiguration configuration)
        {
            var mailSettings = configuration.GetSection("MailSettings");
            fromMail = mailSettings.GetSection("FromMail").Value;
            displayName = mailSettings.GetSection("DisplayName").Value;
            password = mailSettings.GetSection("Password").Value;
            smtpHost = mailSettings.GetSection("SmtpHost").Value;
            smtpPort = Convert.ToInt32(mailSettings.GetSection("SmtpPort").Value);
            frontendUrl = configuration.GetSection("Frontend").GetSection("Address").Value;
        }

        public Task SendEmailVerificationEmail(User user)
        {
            var email = new MimeMessage();
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = VERIFY_EMAIL_SUBJECT;

            var builder = new BodyBuilder();
            builder.HtmlBody = VERIFY_EMAIL_BODY.Replace("{username}", !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : user.Username)
                .Replace("{url}", frontendUrl) // TODO: Replace with actual correct url
                .Replace("{code}", user.EmailVerificationCode);
            email.Body = builder.ToMessageBody();

            return SendEmail(email);
        }

        private async Task SendEmail(MimeMessage email)
        {
            email.Sender = MailboxAddress.Parse(fromMail);
            using (var smtp = new SmtpClient())
            {
                smtp.Connect(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(fromMail, password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }
    }
}
