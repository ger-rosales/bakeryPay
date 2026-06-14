namespace BakeryPay.Mobile.Models;

public class ResetUserPasswordResultModel
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
