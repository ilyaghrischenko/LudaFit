using LudaFit.SharedKernel.Options;

namespace LudaFit.Infrastructure.Gmail.Options;

public sealed record SendEmailOptions(
    string ClientName,
    string ClientEmail,
    string ClientPhoneNumber,
    string? ClientTelegramTag,
    BookingOptions Booking
);
