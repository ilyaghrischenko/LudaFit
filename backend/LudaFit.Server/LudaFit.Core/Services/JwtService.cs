using System.Globalization;
using LudaFit.Core.Settings;
using LudaFit.SharedKernel.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LudaFit.Core.Services;

internal sealed class JwtService(IOptions<JwtSettings> jwtSettings, TimeProvider timeProvider) : IScopedType
{
    private readonly JwtSettings _settings = jwtSettings.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public string GenerateToken(int id, string login)
    {
        DateTime currentDateTime = timeProvider.GetUtcNow().UtcDateTime;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            NotBefore = currentDateTime,
            Expires = currentDateTime.Add(TimeSpan.FromDays(_settings.Lifetime)),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = id.ToString(CultureInfo.InvariantCulture),
                [JwtRegisteredClaimNames.Nickname] = login,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            },
            SigningCredentials = new SigningCredentials(_settings.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256)
        };

        return _handler.CreateToken(descriptor);
    }
}
