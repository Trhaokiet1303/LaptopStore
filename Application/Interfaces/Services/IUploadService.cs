using LaptopStore.Application.Requests;

namespace LaptopStore.Application.Interfaces.Services
{
    public interface IUploadService
    {
        string UploadAsync(UploadRequest request);
    }
}