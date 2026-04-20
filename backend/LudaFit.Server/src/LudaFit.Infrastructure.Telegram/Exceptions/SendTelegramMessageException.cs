namespace LudaFit.Infrastructure.Telegram.Exceptions;

public sealed class SendTelegramMessageException : Exception
{
    public SendTelegramMessageException() { }
    
    public SendTelegramMessageException(string message)
        : base(message) { }

    public SendTelegramMessageException(string message, Exception innerException)
        : base(message, innerException) { }
    
    public SendTelegramMessageException(Exception innerException)
        : base(innerException.Message, innerException) { }
}
