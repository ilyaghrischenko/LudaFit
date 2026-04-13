using System.Net;
using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class EmailOutboxMessage : OutboxMessageEntity
{
    public Booking Booking { get; private set; } = null!;
    public int BookingId { get; private set; }

    private EmailOutboxMessage()
        : base(default) { }

    public EmailOutboxMessage(DateTime currentDateTime, Booking booking)
        : base(currentDateTime)
    {
        Booking = booking;
        BookingId = booking.Id;
    }
}
