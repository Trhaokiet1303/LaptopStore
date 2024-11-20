using LaptopStore.Application.Interfaces.Services;
using System;

namespace LaptopStore.Infrastructure.Shared.Services
{
    public class SystemDateTimeService : IDateTimeService
    {
        public DateTime NowUtc => DateTime.UtcNow;
    }
}