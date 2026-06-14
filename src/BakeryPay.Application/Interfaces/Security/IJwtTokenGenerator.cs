using BakeryPay.Application.DTOs.Auth;
using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    AuthResponseDto GenerateToken(User user);
}
