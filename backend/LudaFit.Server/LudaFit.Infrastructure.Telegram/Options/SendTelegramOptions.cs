using LudaFit.SharedKernel.Options;

namespace LudaFit.Infrastructure.Telegram.Options;

public sealed record SendTelegramOptions(
    string ClientName,
    string ClientEmail,
    string ClientPhoneNumber,
    string? ClientTelegramTag,
    BookingOptions Booking
);
