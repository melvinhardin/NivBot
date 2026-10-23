using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NivBot.Features.Roles
{
    // Lets /roles sync start a sync right away instead of waiting for the next interval.
    public sealed class RoleSyncTrigger
    {
        private readonly SemaphoreSlim signal = new(0, 1);

        public void Request()
        {
            try { signal.Release(); }
            catch (SemaphoreFullException) { /* a sync is already requested */ }
        }

        // Returns when the interval passed or a sync was requested.
        public Task WaitAsync(TimeSpan interval, CancellationToken cancellationToken) =>
            signal.WaitAsync(interval > TimeSpan.Zero ? interval : Timeout.InfiniteTimeSpan, cancellationToken);
    }

    // Runs the role sync every RolesOptions.SyncInterval.
    public sealed class RoleSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        RolesOptions options,
        RoleSyncTrigger trigger,
        ILogger<RoleSyncBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Give the bot a moment to start; without an interval only /roles sync starts a run
                if (options.SyncInterval > TimeSpan.Zero) { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); }
                else { await trigger.WaitAsync(TimeSpan.Zero, stoppingToken); }
            }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<RoleSyncService>().SyncAllAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Role sync failed");
                }

                try
                {
                    await trigger.WaitAsync(options.SyncInterval, stoppingToken);
                }
                catch (OperationCanceledException) { return; }
            }
        }
    }
}
