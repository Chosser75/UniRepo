using Infrastructure.Database.DbContexts;
using Infrastructure.Database.Interfaces;
using Infrastructure.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniRepo.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var dbName = Guid.NewGuid().ToString();
        services
            .AddDbContext<UniRepoContext>(options => options.UseInMemoryDatabase(databaseName: dbName))
            .AddUniRepo()
            .AddScoped<IDbService, DbService>();

        return services;
    }
}