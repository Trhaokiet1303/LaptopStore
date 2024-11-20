using System.Collections.Generic;
using System.Threading.Tasks;
using LaptopStore.Application.Requests.Identity;
using LaptopStore.Application.Responses.Identity;
using LaptopStore.Shared.Wrapper;

namespace LaptopStore.Client.Infrastructure.Managers.Identity.RoleClaims
{
    public interface IRoleClaimManager : IManager
    {
        Task<IResult<List<RoleClaimResponse>>> GetRoleClaimsAsync();

        Task<IResult<List<RoleClaimResponse>>> GetRoleClaimsByRoleIdAsync(string roleId);

        Task<IResult<string>> SaveAsync(RoleClaimRequest role);

        Task<IResult<string>> DeleteAsync(string id);
    }
}