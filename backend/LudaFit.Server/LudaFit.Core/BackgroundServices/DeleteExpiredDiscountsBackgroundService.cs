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
            try
            {
                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

                var db = scope.ServiceProvider.GetRequiredService<LudaFitDbContext>();

                DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

                await db.Discounts
                    .Where(discount => discount.DateRange.End < currentDate)
                    .ExecuteDeleteAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Console.WriteLine(ex);
            }
        }
    }
}
