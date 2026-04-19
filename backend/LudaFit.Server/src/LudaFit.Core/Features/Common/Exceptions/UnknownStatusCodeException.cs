using System.Net;

namespace LudaFit.Core.Features.Common.Exceptions;

internal sealed class UnknownStatusCodeException : Exception
{
    public UnknownStatusCodeException() { }
    
    public UnknownStatusCodeException(HttpStatusCode statusCode)
        : base($"unknown status code: {statusCode}") { }
    
    public UnknownStatusCodeException(string message)
        : base(message) { }
    
    public UnknownStatusCodeException(string message, Exception innerException)
        : base(message, innerException) { }
}
