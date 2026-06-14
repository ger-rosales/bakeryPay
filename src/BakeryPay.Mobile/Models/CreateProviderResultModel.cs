namespace BakeryPay.Mobile.Models;

public class CreateProviderResultModel
{
    public ProviderModel Provider { get; set; } = new();
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}
