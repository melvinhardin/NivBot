using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NivBot.Features.Competitions
{
    // Every 10 minutes: asks WOM for a final update of competitions ending within the hour,
    // and finishes competitions that ended (results post + points).
    public sealed class CompetitionBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CompetitionBackgroundService> logger) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

        // Competitions already asked for a final update while the bot runs
        private readonly HashSet<int> finalUpdateRequested = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    CompetitionService competitions = scope.ServiceProvider.GetRequiredService<CompetitionService>();
                    await competitions.RequestFinalUpdatesAsync(finalUpdateRequested, stoppingToken);
                    await competitions.FinishEndedAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Competition check failed");
                }
            }
            while (await WaitAsync(timer, stoppingToken));
        }

        private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken stoppingToken)
        {
            try
            {
                return await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
