using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace TourismWeb.Services // Đảm bảo namespace đúng
{
    // Các class để binding cấu hình (có thể để ở đây hoặc file riêng)
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class EmailSettings
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly EmailSettings _emailSettings;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>()
                ?? throw new InvalidOperationException("SmtpSettings configuration section is missing.");
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>()
                ?? throw new InvalidOperationException("EmailSettings configuration section is missing.");

            if (string.IsNullOrEmpty(_smtpSettings.Host)) throw new InvalidOperationException("SmtpSettings:Host is not configured.");
            if (_smtpSettings.Port <= 0) throw new InvalidOperationException("SmtpSettings:Port is not configured or is invalid.");
            if (string.IsNullOrEmpty(_smtpSettings.Username)) throw new InvalidOperationException("SmtpSettings:Username is not configured.");
            if (string.IsNullOrEmpty(_smtpSettings.Password)) throw new InvalidOperationException("SmtpSettings:Password is not configured (expected in User Secrets).");
            if (string.IsNullOrEmpty(_emailSettings.SenderEmail)) throw new InvalidOperationException("EmailSettings:SenderEmail is not configured.");
            if (string.IsNullOrEmpty(_emailSettings.SenderName)) throw new InvalidOperationException("EmailSettings:SenderName is not configured.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail)); // Tên người nhận có thể để trống
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Quan trọng: Chọn SecureSocketOptions phù hợp với cổng và server của bạn
                    // Cổng 587 thường dùng SecureSocketOptions.StartTls
                    // Cổng 465 thường dùng SecureSocketOptions.SslOnConnect
                    // SecureSocketOptions.Auto có thể thử, nhưng chỉ định rõ ràng thường tốt hơn.
                    SecureSocketOptions secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable; // Mặc định
                    if (_smtpSettings.Port == 465)
                    {
                        secureSocketOptions = SecureSocketOptions.SslOnConnect;
                    }
                    else if (_smtpSettings.Port == 587)
                    {
                        secureSocketOptions = SecureSocketOptions.StartTls;
                    }


                    Console.WriteLine($"Attempting to connect to SMTP: {_smtpSettings.Host}:{_smtpSettings.Port} using {secureSocketOptions}");
                    await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, secureSocketOptions);

                    Console.WriteLine($"Authenticating SMTP with username: {_smtpSettings.Username}");
                    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

                    Console.WriteLine($"Sending email to {toEmail} with subject: {subject}");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    Console.WriteLine($"Email sent successfully to {toEmail}.");
                }
            }
            catch (SmtpCommandException ex)
            {
                Console.WriteLine($"SmtpCommandException while sending email to {toEmail}: StatusCode={ex.StatusCode}, ErrorCode={ex.ErrorCode}, Message={ex.Message}");
                Console.WriteLine($"Full SmtpCommandException: {ex.ToString()}");
                throw; // Ném lại lỗi để action cha có thể biết và xử lý (ví dụ, không báo gửi thành công)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General exception while sending email to {toEmail}: {ex.ToString()}");
                throw; // Ném lại lỗi
            }
        }
    }
}