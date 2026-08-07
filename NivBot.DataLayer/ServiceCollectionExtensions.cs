using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace NivBot.DataLayer
{
    public static class ServiceCollectionExtensions
    {
        // TODO: Change this to a DbContextFactory for future background jobs
        public static IServiceCollection AddDataLayer(
            this IServiceCollection services, IConfiguration config, bool isDev) => services.AddDbContext<GoodplaceContext>(x =>
            {
                x
                .UseNpgsql(config.GetConnectionString("Goodplace"), y => y.MigrationsAssembly("NivBot"))
                .UseValidationCheckConstraints();
                
                if (isDev)
                    x.EnableSensitiveDataLogging();
            });

    }
}
