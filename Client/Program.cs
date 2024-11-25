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
using LaptopStore.Shared.Constants.Localization;
using LaptopStore.Application.Interfaces.Common;
using MudBlazor.Services;
using MediatR;
using LaptopStore.Infrastructure.Services;
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

            var host = builder.Build();
            var storageService = host.Services.GetRequiredService<ClientPreferenceManager>();

            if (storageService != null)
            {
                CultureInfo culture;
                var preference = await storageService.GetPreference() as ClientPreference;
                if (preference != null)
                    culture = new CultureInfo(preference.LanguageCode);
                else
                    culture = new CultureInfo(LocalizationConstants.SupportedLanguages.FirstOrDefault()?.Code ?? "en-US");

                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }

            await host.RunAsync();
        }
    }
}
