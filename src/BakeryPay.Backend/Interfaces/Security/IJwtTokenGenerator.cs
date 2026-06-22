using BakeryPay.Backend.DTOs.Auth;
using BakeryPay.Backend.Entities;

namespace BakeryPay.Backend.Interfaces.Security;

public interface IJwtTokenGenerator
{
    AuthResponseDto GenerateToken(User user);
}
