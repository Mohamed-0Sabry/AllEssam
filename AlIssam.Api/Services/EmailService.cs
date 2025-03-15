using MimeKit;
using AlIssam.API.Common;
using AlIssam.API.Config;
using AlIssam.API.Services.InterFaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace AlIssam.API.Services
{
    /// <summary>
    /// Handles email sending functionality using SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailServiceConfigruation _emailConfigruation;

        public EmailService(IOptions<EmailServiceConfigruation> emailConfigruation)
        {
            _emailConfigruation = emailConfigruation.Value;
        }

        /// <summary>
        /// Sends an email message using configured SMTP settings
        /// </summary>
        /// <param name="message">The message to send containing recipient, subject and content</param>
        /// <exception cref="SmtpException">Thrown when SMTP communication fails</exception>
        public async Task SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);

            await Send(emailMessage);
        }

        private async Task Send(MimeMessage mimeMessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(_emailConfigruation.SmptServer,
                                _emailConfigruation.Port, SecureSocketOptions.Auto);

                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailConfigruation.UserName, _emailConfigruation.Password);
                await client.SendAsync(mimeMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
        private MimeMessage CreateEmailMessage(Message message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_emailConfigruation.UserName, _emailConfigruation.From));
            mimeMessage.To.Add(new MailboxAddress(message.To, message.Email));
            mimeMessage.Subject = message.Subject;

            mimeMessage.Body = new TextPart("plain")
            {
                Text = message.Content
            };

            return mimeMessage;
        }
    }
}
