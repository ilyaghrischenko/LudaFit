namespace LudaFit.Infrastructure.Gmail.Exceptions;

public sealed class SendEmailException : Exception
{
    public SendEmailException() { }
    
    public SendEmailException(string message)
        : base(message) { }

    public SendEmailException(string message, Exception innerException)
        : base(message, innerException) { }
    
    public SendEmailException(Exception innerException)
        : base(innerException.Message, innerException) { }
}
