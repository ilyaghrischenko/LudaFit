using LudaFit.Core.Mapping;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.Gmail;
using LudaFit.Infrastructure.Gmail.Options;
using LudaFit.Infrastructure.SQLite;
using LudaFit.Infrastructure.Telegram;
using LudaFit.Infrastructure.Telegram.Options;
using LudaFit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.BackgroundServices;

internal sealed class SendTelegramBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                
                var db = scope.ServiceProvider.GetRequiredService<LudaFitDbContext>();
                var telegramSender = scope.ServiceProvider.GetRequiredService<TelegramSender>();
                var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                DateTime currentDateTime = timeProvider.GetUtcNow().UtcDateTime;
                
                List<TelegramOutboxMessage> telegramMessages = await db.TelegramOutboxMessages
                    .Include(telegramMessage => telegramMessage.Booking)
                    .Where(telegramMessage => telegramMessage.AttemptsCount < telegramMessage.MaxAttemptNumber
                                              && telegramMessage.NextAttemptAtUtc <= currentDateTime)
                    .ToListAsync(stoppingToken);

                foreach (TelegramOutboxMessage telegramMessage in telegramMessages)
                {
                    Result addAnotherTryResult = telegramMessage.AddAnotherTry();

                    if (addAnotherTryResult.IsFailure)
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

                    var isSentSuccessfully = false;

                    try
                    {
                        Result sendEmailResult = await telegramSender.SendAsync(options, stoppingToken);
                        isSentSuccessfully = sendEmailResult.IsSuccess;
                    }
#pragma warning disable CA1031
                    catch (Exception ex)
#pragma warning restore CA1031
                    {
                        Console.WriteLine(ex);
                    }

                    if (isSentSuccessfully || telegramMessage.UsedAllAttempts)
                    {
                        db.TelegramOutboxMessages.Remove(telegramMessage);
                    }
                    
                    await db.SaveChangesAsync(stoppingToken);
                }
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
                    await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
