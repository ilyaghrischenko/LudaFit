using System.Data.Common;
using LudaFit.Infrastructure.SQLite;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.BackgroundServices;

internal sealed partial class DeleteExpiredDiscountsBackgroundService(
    IServiceProvider serviceProvider,
    TimeProvider timeProvider,
    ILogger<DeleteExpiredDiscountsBackgroundService> logger) : BackgroundService
{
    //todo: написать заметку про ПРАВИЛЬНОЕ логирование
    [LoggerMessage(1, LogLevel.Error, "Error while deleting expired discounts. Date: {Date}")]
    private partial void LogDbError(DbException ex, DateOnly date);
    
    [LoggerMessage(2, LogLevel.Information, "Operation cancelled. Date: {Date}")]
    private partial void LogCancelledOperation(DateOnly date);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

            var db = scope.ServiceProvider.GetRequiredService<LudaFitDbContext>();

            DateTime now = timeProvider.GetUtcNow().UtcDateTime;
            DateTime nextMidnight = now.Date.AddDays(1);
                
            TimeSpan timeUntilMidnight = nextMidnight - now;
                
            DateOnly currentDate = DateOnly.FromDateTime(now);

            try
            {
                await db.Discounts
                    .Where(discount => discount.DateRange.End < currentDate)
                    .ExecuteDeleteAsync(stoppingToken);
            }
            catch (DbException ex)
            {
                // ignored
                LogDbError(ex, currentDate);
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
                    LogCancelledOperation(currentDate);
                }
            }
        }
    }
}
