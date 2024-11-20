using LaptopStore.Application.Interfaces.Common;
using LaptopStore.Application.Requests.Identity;
using LaptopStore.Shared.Wrapper;
using System.Threading.Tasks;

namespace LaptopStore.Application.Interfaces.Services.Account
{
    public interface IAccountService : IService
    {
        Task<IResult> UpdateProfileAsync(UpdateProfileRequest model, string userId);

        Task<IResult> ChangePasswordAsync(ChangePasswordRequest model, string userId);

        Task<IResult<string>> GetProfilePictureAsync(string userId);

        Task<IResult<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request, string userId);
    }
}