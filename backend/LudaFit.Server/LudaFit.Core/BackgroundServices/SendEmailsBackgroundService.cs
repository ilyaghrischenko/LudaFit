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
                
                List<EmailOutboxMessage> unsentEmails = await db.EmailOutboxMessages
                    .Include(unsentEmail => unsentEmail.Booking)
                    .Where(unsentEmail => unsentEmail.AttemptNumber < unsentEmail.MaxAttemptNumber && unsentEmail.NextAttemptAt <= currentDateTime)
                    .ToListAsync(stoppingToken);

                foreach (EmailOutboxMessage unsentEmail in unsentEmails)
                {
                    Result addAnotherTryResult = unsentEmail.AddAnotherTry();

                    if (addAnotherTryResult.IsFailure)
                    {
                        db.EmailOutboxMessages.Remove(unsentEmail);
                        await db.SaveChangesAsync(stoppingToken);
                        continue;
                    }

                    Booking booking = unsentEmail.Booking;

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
                    catch (Exception)
#pragma warning restore CA1031
                    {
                        // ignored
                    }

                    if (isSentSuccessfully || unsentEmail.UsedAllAttempts)
                    {
                        db.EmailOutboxMessages.Remove(unsentEmail);
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
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
            }
        }
    }
}
