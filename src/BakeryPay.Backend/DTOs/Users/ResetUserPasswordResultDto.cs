namespace BakeryPay.Backend.DTOs.Users;

public class ResetUserPasswordResultDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
