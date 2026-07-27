using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace NivBot.DataLayer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataLayer(
            this IServiceCollection services, IConfiguration config, bool isDev) => services.AddDbContext<GoodplaceContext>(o =>
            {
                o
                .UseNpgsql(config.GetConnectionString("Goodplace"))
                .UseValidationCheckConstraints();

                if (isDev)
                    o.EnableSensitiveDataLogging();
            });
    }
}
