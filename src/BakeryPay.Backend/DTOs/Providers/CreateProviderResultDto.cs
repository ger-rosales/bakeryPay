namespace BakeryPay.Backend.DTOs.Providers;

public class CreateProviderResultDto
{
    public ProviderDto Provider { get; set; } = new();
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
