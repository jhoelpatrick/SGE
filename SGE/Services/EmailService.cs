using System.Net;
using System.Net.Mail;

namespace SGE.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpSec = _config.GetSection("SmtpSettings");
            bool simulate = smtpSec.GetValue<bool>("Simulate");

            if (simulate)
            {
                Console.WriteLine($"[SMTP SIMULATION] To: {toEmail}, Subject: {subject}, Body: {htmlBody}");
                // In simulation, we also write to console and log
                return true;
            }

            try
            {
                string server = smtpSec["Server"] ?? "";
                int port = smtpSec.GetValue<int>("Port");
                string senderName = smtpSec["SenderName"] ?? "SGE Enterprise";
                string senderEmail = smtpSec["SenderEmail"] ?? "";
                string username = smtpSec["Username"] ?? "";
                string password = smtpSec["Password"] ?? "";
                bool useSsl = smtpSec.GetValue<bool>("UseSsl");

                // Redirection Hook for Demo:
                // If destination ends with corporate domain, redirect it to zaiduriarteleo@gmail.com
                string actualToEmail = toEmail;
                string adjustedSubject = subject;
                string adjustedBody = htmlBody;

                string allowedDomain = _config["AuthSettings:AllowedDomain"] ?? "sge-enterprise.com";
                if (toEmail.EndsWith("@" + allowedDomain, System.StringComparison.OrdinalIgnoreCase))
                {
                    actualToEmail = "zaiduriarteleo@gmail.com";
                    adjustedSubject = $"[DEMO REDIRIGIDO - {toEmail}] {subject}";
                    adjustedBody = $@"
                        <div style='background-color: #fffbeb; border: 1px solid #fef3c7; color: #92400e; padding: 12px; margin-bottom: 16px; border-radius: 6px; font-family: sans-serif; font-size: 14px;'>
                            <strong>Modo Demostración:</strong> Este correo fue originalmente enviado a la cuenta corporativa <code>{toEmail}</code> (que no existe físicamente), pero ha sido redirigido a tu Gmail para fines de presentación.
                        </div>
                        {htmlBody}";
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = adjustedSubject,
                    Body = adjustedBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(actualToEmail);

                using var smtpClient = new SmtpClient(server, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = useSsl || port == 465 || port == 587
                };

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
