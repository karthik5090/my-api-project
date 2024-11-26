using Microsoft.AspNetCore.Identity.UI.Services; // Ensure this namespace is correct
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ForgotPasswordApp.Services
{
    // Make sure this class implements the IEmailSender interface
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587; // TLS
        private readonly string _smtpUsername = "mystore5061@gmail.com";  // Replace with your email
        private readonly string _smtpPassword = "ccqwzzfsppxitcvp";  // Replace with your email password

        // Implementing the SendEmailAsync method from IEmailSender
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient(_smtpServer)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUsername),
                Subject = subject,
                Body = message,
            };

            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
