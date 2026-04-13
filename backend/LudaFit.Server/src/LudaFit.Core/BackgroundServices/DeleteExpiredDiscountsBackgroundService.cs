using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.BackgroundServices;

internal sealed class DeleteExpiredDiscountsBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan timeUntilMidnight = TimeSpan.FromDays(1);
            
            try
            {
                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

                var db = scope.ServiceProvider.GetRequiredService<LudaFitDbContext>();
                var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                DateTime now = timeProvider.GetUtcNow().UtcDateTime;
                DateTime nextMidnight = now.Date.AddDays(1);
                DateOnly currentDate = DateOnly.FromDateTime(now);
                
                timeUntilMidnight = nextMidnight - now;

                await db.Discounts
                    .Where(discount => discount.DateRange.End < currentDate)
                    .ExecuteDeleteAsync(stoppingToken);
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Console.WriteLine(ex);
            }
            finally
            {
                try
                {
                    await Task.Delay(timeUntilMidnight, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
            }
        }
    }
}
