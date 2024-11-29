using AutoMapper;
using LaptopStore.Application.Interfaces.Services;
using LaptopStore.Infrastructure.Models.Audit;
using LaptopStore.Application.Responses.Audit;
using LaptopStore.Infrastructure.Contexts;
using LaptopStore.Shared.Wrapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LaptopStore.Application.Extensions;
using LaptopStore.Infrastructure.Specifications;
using Microsoft.Extensions.Localization;

namespace LaptopStore.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly DBContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AuditService> _localizer;

        public AuditService(
            IMapper mapper,
            DBContext context,
            IStringLocalizer<AuditService> localizer)
        {
            _mapper = mapper;
            _context = context;
            _localizer = localizer;
        }

        public async Task<IResult<IEnumerable<AuditResponse>>> GetCurrentUserTrailsAsync(string userId)
        {
            var trails = await _context.AuditTrails.Where(a => a.UserId == userId).OrderByDescending(a => a.Id).Take(250).ToListAsync();
            var mappedLogs = _mapper.Map<List<AuditResponse>>(trails);
            return await Result<IEnumerable<AuditResponse>>.SuccessAsync(mappedLogs);
        }

    }
}