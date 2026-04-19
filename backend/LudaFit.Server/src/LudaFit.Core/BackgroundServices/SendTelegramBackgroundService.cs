using System.Data.Common;
using LudaFit.Core.Mapping;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.SQLite;
using LudaFit.Infrastructure.Telegram;
using LudaFit.Infrastructure.Telegram.Options;
using LudaFit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.BackgroundServices;

internal sealed partial class SendTelegramBackgroundService(
    IServiceProvider serviceProvider,
    TimeProvider timeProvider,
    ILogger<SendTelegramBackgroundService> logger) : BackgroundService
{
    [LoggerMessage(1, LogLevel.Error, "Error while deleting expired discounts. DateTime: {DateTime}")]
    private partial void LogDbError(DbException ex, DateTime dateTime);
    
    [LoggerMessage(2, LogLevel.Information, "Operation cancelled. DateTime: {DateTime}")]
    private partial void LogCancelledOperation(DateTime dateTime);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime currentDateTime = timeProvider.GetUtcNow().UtcDateTime;

            try
            {
                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

                var db = scope.ServiceProvider.GetRequiredService<LudaFitDbContext>();
                var telegramSender = scope.ServiceProvider.GetRequiredService<TelegramSender>();

                List<TelegramOutboxMessage> telegramMessages = await db.TelegramOutboxMessages
                    .Include(telegramMessage => telegramMessage.Booking)
                    .Where(telegramMessage => telegramMessage.AttemptsCount < telegramMessage.MaxAttemptNumber
                                              && telegramMessage.NextAttemptAtUtc <= currentDateTime)
                    .ToListAsync(stoppingToken);

                foreach (TelegramOutboxMessage telegramMessage in telegramMessages)
                {
                    bool isAnotherAttemptAvailable = telegramMessage.IsAnotherAttemptAvailable();

                    if (isAnotherAttemptAvailable is false)
                    {
                        db.TelegramOutboxMessages.Remove(telegramMessage);
                        await db.SaveChangesAsync(stoppingToken);
                        continue;
                    }

                    Booking booking = telegramMessage.Booking;

                    SendTelegramOptions options = new(
                        booking.FullName,
                        booking.ClientContacts.Email.Address,
                        booking.ClientContacts.PhoneNumber,
                        booking.ClientContacts.TelegramTag,
                        booking.ToEmailOptions()
                    );

                    Result sendEmailResult = await telegramSender.SendAsync(options, stoppingToken);

                    if (sendEmailResult.IsSuccess || telegramMessage.UsedAllAttempts)
                    {
                        db.TelegramOutboxMessages.Remove(telegramMessage);
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (DbException ex)
            {
                // ignored
                LogDbError(ex, currentDateTime);
            }
            finally
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignored
                    LogCancelledOperation(currentDateTime);
                }
            }
        }
    }
}
