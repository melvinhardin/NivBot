using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace NivBot.DataLayer
{
    public static class ServiceCollectionExtensions
    {
        // Scoped DbContext: the background jobs (role sync, competition results) create a scope per run
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
