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

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

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
