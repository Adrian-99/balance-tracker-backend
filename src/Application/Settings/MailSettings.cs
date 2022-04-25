using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    internal class MailSettings
    {
        public string FromMail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public TemplateSettings EmailVerificationTemplate { get; set; }
        public TemplateSettings ResetPasswordTemplate { get; set; }

        public static MailSettings Get(IConfiguration configuration)
        {
            var mailSettings = configuration.GetSection("Mail")
                .Get<MailSettings>();
            mailSettings.EmailVerificationTemplate = TemplateSettings.Get(configuration, "EmailVerification");
            mailSettings.ResetPasswordTemplate = TemplateSettings.Get(configuration, "ResetPassword");
            return mailSettings;
        }
    }

    internal class TemplateSettings
    {
        public string Subject { get; set; }
        public string TemplatePath { get; set; }

        public string TemplateContent { get => File.ReadAllText(TemplatePath); }

        public static TemplateSettings Get(IConfiguration configuration, string templateName)
        {
            return configuration.GetSection("Mail")
                .GetSection("Templates")
                .GetSection(templateName)
                .Get<TemplateSettings>();
        }
    }
}
