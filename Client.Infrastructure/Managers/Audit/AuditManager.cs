using LaptopStore.Application.Responses.Audit;
using LaptopStore.Client.Infrastructure.Extensions;
using LaptopStore.Shared.Wrapper;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LaptopStore.Client.Infrastructure.Managers.Audit
{
    public class AuditManager : IAuditManager
    {
        private readonly HttpClient _httpClient;

        public AuditManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResult<IEnumerable<AuditResponse>>> GetAllTrailsAsync()
        {
            var response = await _httpClient.GetAsync(Routes.AuditEndpoints.GetAllTrails);
            var data = await response.ToResult<IEnumerable<AuditResponse>>();
            return data;
        }

    }
}