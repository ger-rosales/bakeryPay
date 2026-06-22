using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BakeryPay.Backend.DTOs.Auth;
using BakeryPay.Backend.Interfaces.Security;
using BakeryPay.Backend.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BakeryPay.Backend.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public AuthResponseDto GenerateToken(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var roleName = user.Role?.Name ?? string.Empty;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Role, roleName)
        };

        if (user.ProviderId.HasValue)
        {
            claims.Add(new Claim("provider_id", user.ProviderId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            IdentificationNumber = user.IdentificationNumber,
            Email = user.Email,
            Phone = user.Phone,
            Role = roleName,
            ProviderId = user.ProviderId,
            ProviderName = user.Provider?.BusinessName,
            BiometricEnabled = user.BiometricCredentials.Any(x => x.IsActive),
            HasAcceptedPolicies = user.HasAcceptedPolicies,
            MustChangePassword = user.MustChangePassword
        };
    }
}
