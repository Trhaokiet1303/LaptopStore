using AutoMapper;
using LaptopStore.Infrastructure.Models.Audit;
using LaptopStore.Application.Responses.Audit;

namespace LaptopStore.Infrastructure.Mappings
{
    public class AuditProfile : Profile
    {
        public AuditProfile()
        {
            CreateMap<AuditResponse, Audit>().ReverseMap();
        }
    }
}