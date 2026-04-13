using System.Net;
using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class EmailOutboxMessage : BaseEntity
{
    public int AttemptNumber { get; private set; }

    public int MaxAttemptNumber { get; } = 5;

    public bool UsedAllAttempts => AttemptNumber == MaxAttemptNumber;
    
    public DateTime NextAttemptAt { get; private set; }

    public Booking Booking { get; private set; } = null!;
    public int BookingId { get; private set; }

    private EmailOutboxMessage() { }

    public EmailOutboxMessage(DateTime currentDateTime, Booking booking)
    {
        NextAttemptAt = currentDateTime;
        Booking = booking;
        BookingId = booking.Id;
    }

    public Result AddAnotherTry()
    {
        if (UsedAllAttempts)
        {
            return new ErrorDetails("Всі спроби вичерпано");
        }
        
        AttemptNumber += 1;

        NextAttemptAt = AttemptNumber switch
        {
            1 => NextAttemptAt.AddMinutes(15),
            2 => NextAttemptAt.AddHours(1),
            3 => NextAttemptAt.AddHours(5),
            4 => NextAttemptAt.AddDays(1),
            _ => NextAttemptAt
        };
        
        return Result.Success();
    }
}
