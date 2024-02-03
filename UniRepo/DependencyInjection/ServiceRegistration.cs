using Microsoft.Extensions.DependencyInjection;
using UniRepo.Interfaces;
using UniRepo.Repositories;

namespace UniRepo.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddUniRepo(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUniversalRepository<,>), typeof(UniversalRepository<,>));

        return services;
    }
}