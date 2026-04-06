using System.Net.Mail;

namespace LudaFit.Core.Options;

internal sealed record EmailOptions
{
    public required string Email { get; set; }
}
