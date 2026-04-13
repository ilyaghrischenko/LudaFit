using LudaFit.Domain.Entities.Common;

namespace LudaFit.Domain.Entities;

public sealed class TelegramOutboxMessage : OutboxMessageEntity
{
    public Booking Booking { get; private set; } = null!;
    public int BookingId { get; private set; }

    private TelegramOutboxMessage()
        : base(default) { }

    public TelegramOutboxMessage(DateTime currentDateTime, Booking booking)
        : base(currentDateTime)
    {
        Booking = booking;
        BookingId = booking.Id;
    }
}
