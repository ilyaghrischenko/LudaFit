using System.Data.Common;
using LudaFit.Core.Mapping;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.Gmail;
using LudaFit.Infrastructure.Gmail.Options;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.BackgroundServices;

internal sealed partial class SendEmailsBackgroundService(
    IServiceProvider serviceProvider,
    TimeProvider timeProvider,
    ILogger<SendEmailsBackgroundService> logger) : BackgroundService
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
                var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

                List<EmailOutboxMessage> emailsToSent = await db.EmailOutboxMessages
                    .Include(emailToSent => emailToSent.Booking)
                    .Where(emailToSent => emailToSent.AttemptsCount < emailToSent.MaxAttemptNumber
                                          && emailToSent.NextAttemptAtUtc <= currentDateTime)
                    .ToListAsync(stoppingToken);

                foreach (EmailOutboxMessage emailToSent in emailsToSent)
                {
                    bool isAnotherAttemptAvailable = emailToSent.IsAnotherAttemptAvailable();

                    if (isAnotherAttemptAvailable is false)
                    {
                        db.EmailOutboxMessages.Remove(emailToSent);
                        await db.SaveChangesAsync(stoppingToken);
                        continue;
                    }

                    Booking booking = emailToSent.Booking;

                    SendEmailOptions options = new(
                        booking.FullName,
                        booking.ClientContacts.Email.Address,
                        booking.ClientContacts.PhoneNumber,
                        booking.ClientContacts.TelegramTag,
                        booking.ToEmailOptions()
                    );

                    Result sendEmailResult = await emailSender.SendAsync(options, stoppingToken);

                    if (sendEmailResult.IsSuccess || emailToSent.UsedAllAttempts)
                    {
                        db.EmailOutboxMessages.Remove(emailToSent);
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
