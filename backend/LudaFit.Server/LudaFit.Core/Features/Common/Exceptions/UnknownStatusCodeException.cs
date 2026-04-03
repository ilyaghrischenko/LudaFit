using System.Net;

namespace LudaFit.Core.Features.Common.Exceptions;

#pragma warning disable CA1812
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
#pragma warning restore CA1812
