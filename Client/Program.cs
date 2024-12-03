using LaptopStore.Client.Extensions;
using LaptopStore.Client.Infrastructure.Managers.Preferences;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Infrastructure.Repositories;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LaptopStore.Client.Infrastructure.Settings;
using LaptopStore.Application.Interfaces.Common;
using MudBlazor.Services;
using MediatR;
using LaptopStore.Infrastructure.Services;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Order;
namespace LaptopStore.Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args)
                                                .AddRootComponents()
                                                .AddClientServices();

            // Đăng ký các dịch vụ vào builder.Services sau khi builder được khởi tạo
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductManager, ProductManager>();
            builder.Services.AddScoped<ProductManager>();
            builder.Services.AddMudServices();
            builder.Services.AddScoped<IOrderManager, OrderManager>();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
