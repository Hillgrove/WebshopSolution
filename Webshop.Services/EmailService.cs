using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Webshop.Services
{
    public class EmailService
    {
        public async Task SendPasswordResetEmail(string email, string resetLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Webshop", "no-reply@webshop.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Password Reset Request";
            message.Body = new TextPart("plain")
            {
                Text = $"Please reset your password by clicking the following link: {resetLink}"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.webshop.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("no-reply@webshop.com", "password");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendPasswordChangedNotification(string email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Webshop", "no-reply@webshop.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Password Changed";
            message.Body = new TextPart("plain")
            {
                Text = "Your password has been changed. If you did not request this change, please contact support immediately."
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.webshop.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("no-reply@webshop.com", "password");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
