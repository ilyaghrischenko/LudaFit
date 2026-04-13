using LudaFit.Core.Mapping;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.Gmail;
using LudaFit.Infrastructure.Gmail.Options;
using LudaFit.Infrastructure.SQLite;
using LudaFit.SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace LudaFit.Core.BackgroundServices;

internal sealed class SendEmailsBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                
                var db = scope.ServiceProvider.GetRequiredService<LudaFitDbContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
                var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                DateTime currentDateTime = timeProvider.GetUtcNow().UtcDateTime;
                
                List<EmailOutboxMessage> emailsToSent = await db.EmailOutboxMessages
                    .Include(emailToSent => emailToSent.Booking)
                    .Where(emailToSent => emailToSent.AttemptsCount < emailToSent.MaxAttemptNumber
                                          && emailToSent.NextAttemptAtUtc <= currentDateTime)
                    .ToListAsync(stoppingToken);

                foreach (EmailOutboxMessage emailToSent in emailsToSent)
                {
                    Result addAnotherTryResult = emailToSent.AddAnotherTry();

                    if (addAnotherTryResult.IsFailure)
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

                    var isSentSuccessfully = false;

                    try
                    {
                        Result sendEmailResult = await emailSender.SendAsync(options, stoppingToken);
                        isSentSuccessfully = sendEmailResult.IsSuccess;
                    }
#pragma warning disable CA1031
                    catch (Exception ex)
#pragma warning restore CA1031
                    {
                        Console.WriteLine(ex);
                    }

                    if (isSentSuccessfully || emailToSent.UsedAllAttempts)
                    {
                        db.EmailOutboxMessages.Remove(emailToSent);
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
