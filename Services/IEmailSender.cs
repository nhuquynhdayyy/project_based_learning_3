using System.Threading.Tasks;

namespace TourismWeb.Services // Đảm bảo namespace đúng
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}