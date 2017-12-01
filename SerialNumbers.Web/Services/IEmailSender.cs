using System.Threading.Tasks;

namespace SerialNumbers.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}