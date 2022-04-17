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
            "<b>{code}</b><br/><br/>" +
            "Uwaga! Kod ten jest aktywny tylko przez {validFor} minut! Po tym czasie konieczne będzie wygenerowanie nowego kodu.";
        private static readonly string RESET_PASSWORD_SUBJECT = "Balance Tracker - resetowanie hasła";
        private static readonly string RESET_PASSWORD_BODY = "Witaj {username}!<br/><br/>" +
            "Ktoś poprosił o reset hasła do Twojego konta.<br/>" +
            "Możesz ustawić nowe hasło klikając w <a href=\"{url}\">ten link</a> lub wprowadzając poniższy kod:<br/>" +
            "<b>{code}</b><br/><br/>" +
            "Uwaga! Kod ten jest aktywny tylko przez {validFor} minut! Po tym czasie konieczne będzie wygenerowanie nowego kodu.";

        private readonly string fromMail;
        private readonly string displayName;
        private readonly string password;
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string frontendUrl;
        private readonly string emailVerificationCodeValidMinutes;
        private readonly string resetPasswordCodeValidMinutes;

        public MailService(IConfiguration configuration)
        {
            var mailSettings = configuration.GetSection("MailSettings");
            fromMail = mailSettings["FromMail"];
            displayName = mailSettings["DisplayName"];
            password = mailSettings["Password"];
            smtpHost = mailSettings["SmtpHost"];
            smtpPort = Convert.ToInt32(mailSettings["SmtpPort"]);

            frontendUrl = configuration["Frontend:Address"];
            emailVerificationCodeValidMinutes = configuration["UserSettings:EmailVerificationCode:ValidMinutes"];
            resetPasswordCodeValidMinutes = configuration["UserSettings:ResetPasswordCode:ValidMinutes"];
        }

        public Task SendEmailVerificationEmailAsync(User user)
        {
            var placeholdersMap = new Dictionary<string, string>();
            placeholdersMap.Add("{username}", !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : user.Username);
            placeholdersMap.Add("{url}", frontendUrl); // TODO: Replace with actual correct url
            placeholdersMap.Add("{code}", user.EmailVerificationCode);
            placeholdersMap.Add("{validFor}", emailVerificationCodeValidMinutes);

            return SendEmail(user.Email, VERIFY_EMAIL_SUBJECT, VERIFY_EMAIL_BODY, placeholdersMap);
        }

        public Task SendResetPasswordEmailAsync(User user)
        {
            var placeholdersMap = new Dictionary<string, string>();
            placeholdersMap.Add("{username}", !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : user.Username);
            placeholdersMap.Add("{url}", $"{frontendUrl}/reset-password?code={user.ResetPasswordCode}");
            placeholdersMap.Add("{code}", user.ResetPasswordCode);
            placeholdersMap.Add("{validFor}", resetPasswordCodeValidMinutes);

            return SendEmail(user.Email, RESET_PASSWORD_SUBJECT, RESET_PASSWORD_BODY, placeholdersMap);
        }

        private async Task SendEmail(string emailTo, string subject, string body, Dictionary<string, string>? placeholdersMap = null)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(fromMail);
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            if (placeholdersMap != null)
            {
                foreach (var placeholder in placeholdersMap)
                {
                    body = body.Replace(placeholder.Key, placeholder.Value);
                }
            }

            var builder = new BodyBuilder();
            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();

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
