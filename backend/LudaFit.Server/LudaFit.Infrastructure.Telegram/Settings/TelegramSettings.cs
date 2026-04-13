namespace LudaFit.Infrastructure.Telegram.Settings;

public sealed record TelegramSettings
{
    public required string Token { get; set; }
    
    public required long ChatId { get; set; }
}
