using LaptopStore.Application.Interfaces.Common;

namespace LaptopStore.Application.Interfaces.Services
{
    public interface ICurrentUserService : IService
    {
        string UserId { get; }
    }
}