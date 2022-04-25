using Application.Interfaces;
using Application.Settings;
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
        private readonly MailSettings mailSettings;
        private readonly FrontendSettings frontendSettings;
        private readonly EmailVerificationCodeSettings emailVerificationCodeSettings;
        private readonly ResetPasswordCodeSettings resetPasswordCodeSettings;

        public MailService(IConfiguration configuration)
        {
            mailSettings = MailSettings.Get(configuration);
            frontendSettings = FrontendSettings.Get(configuration);
            emailVerificationCodeSettings = EmailVerificationCodeSettings.Get(configuration);
            resetPasswordCodeSettings = ResetPasswordCodeSettings.Get(configuration);
        }

        public Task SendEmailVerificationEmailAsync(User user)
        {
            var placeholdersMap = new Dictionary<string, string>();
            placeholdersMap.Add("{username}", !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : user.Username);
            placeholdersMap.Add("{url}", frontendSettings.Address); // TODO: Replace with actual correct url
            placeholdersMap.Add("{code}", user.EmailVerificationCode);
            placeholdersMap.Add("{validFor}", emailVerificationCodeSettings.ValidMinutes.ToString());

            return SendEmail(user.Email, mailSettings.EmailVerificationTemplate, placeholdersMap);
        }

        public Task SendResetPasswordEmailAsync(User user)
        {
            var placeholdersMap = new Dictionary<string, string>();
            placeholdersMap.Add("{username}", !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : user.Username);
            placeholdersMap.Add("{url}", $"{frontendSettings.Address}/reset-password?code={user.ResetPasswordCode}");
            placeholdersMap.Add("{code}", user.ResetPasswordCode);
            placeholdersMap.Add("{validFor}", resetPasswordCodeSettings.ValidMinutes.ToString());

            return SendEmail(user.Email, mailSettings.ResetPasswordTemplate, placeholdersMap);
        }

        private async Task SendEmail(string emailTo, TemplateSettings templateSettings, Dictionary<string, string>? placeholdersMap = null)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(mailSettings.FromMail);
            email.Sender.Name = mailSettings.DisplayName;
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = templateSettings.Subject;

            var body = templateSettings.TemplateContent;

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
                smtp.Connect(mailSettings.SmtpHost, mailSettings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(mailSettings.FromMail, mailSettings.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }
    }
}
