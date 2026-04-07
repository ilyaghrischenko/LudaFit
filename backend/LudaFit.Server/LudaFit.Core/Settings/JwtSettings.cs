using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LudaFit.Core.Settings;

#pragma warning disable CA1812
internal sealed record JwtSettings
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int Lifetime { get; set; }
    public required string Key { get; set; }
    
    public SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}
#pragma warning restore CA1812
