using LaptopStore.Application.Requests.Mail;
using System.Threading.Tasks;

namespace LaptopStore.Application.Interfaces.Services
{
    public interface IMailService
    {
        Task SendAsync(MailRequest request);
    }
}