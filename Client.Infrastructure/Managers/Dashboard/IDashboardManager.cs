using LaptopStore.Shared.Wrapper;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Dashboards.Queries.GetData;

namespace LaptopStore.Client.Infrastructure.Managers.Dashboard
{
    public interface IDashboardManager : IManager
    {
        Task<IResult<DashboardDataResponse>> GetDataAsync();
    }
}