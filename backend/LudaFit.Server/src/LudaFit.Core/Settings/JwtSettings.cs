using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LudaFit.Core.Settings;

internal sealed record JwtSettings
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int Lifetime { get; set; }
    public required string Key { get; set; }

    private SymmetricSecurityKey? _symmetricSecurityKey;
    public SymmetricSecurityKey SymmetricSecurityKey =>
        _symmetricSecurityKey ??= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
}
