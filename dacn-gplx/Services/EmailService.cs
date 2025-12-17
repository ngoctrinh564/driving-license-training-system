using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace dacn_gplx.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            string senderEmail = emailSettings["SenderEmail"];
            string senderName = emailSettings["SenderName"];
            string senderPassword = emailSettings["SenderPassword"];
            string smtpServer = emailSettings["SmtpServer"];
            int smtpPort = int.Parse(emailSettings["SmtpPort"]);

            var mail = new MailMessage();
            mail.From = new MailAddress(senderEmail, senderName);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using (var smtp = new SmtpClient(smtpServer, smtpPort))
            {
                smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
        }
    }
}
