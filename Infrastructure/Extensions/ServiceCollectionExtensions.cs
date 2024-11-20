using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LaptopStore.Application.Interfaces.Repositories;
using LaptopStore.Application.Interfaces.Services.Storage;
using LaptopStore.Application.Interfaces.Services.Storage.Provider;
using LaptopStore.Application.Interfaces.Serialization.Serializers;
using LaptopStore.Application.Serialization.JsonConverters;
using LaptopStore.Infrastructure.Repositories;
using LaptopStore.Infrastructure.Services.Storage;
using LaptopStore.Application.Serialization.Options;
using LaptopStore.Infrastructure.Services.Storage.Provider;
using LaptopStore.Application.Serialization.Serializers;

namespace LaptopStore.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructureMappings(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddTransient(typeof(IRepositoryAsync<,>), typeof(RepositoryAsync<,>))
                .AddTransient<IProductRepository, ProductRepository>()
                .AddTransient<IBrandRepository, BrandRepository>()
                .AddTransient(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        }

        public static IServiceCollection AddServerStorage(this IServiceCollection services)
            => AddServerStorage(services, null);

        public static IServiceCollection AddServerStorage(this IServiceCollection services, Action<SystemTextJsonOptions> configure)
        {
            return services
                .AddScoped<IJsonSerializer, SystemTextJsonSerializer>()
                .AddScoped<IStorageProvider, ServerStorageProvider>()
                .AddScoped<IServerStorageService, ServerStorageService>()
                .AddScoped<ISyncServerStorageService, ServerStorageService>()
                .Configure<SystemTextJsonOptions>(configureOptions =>
                {
                    configure?.Invoke(configureOptions);
                    if (!configureOptions.JsonSerializerOptions.Converters.Any(c => c.GetType() == typeof(TimespanJsonConverter)))
                        configureOptions.JsonSerializerOptions.Converters.Add(new TimespanJsonConverter());
                });
        }
    }
}