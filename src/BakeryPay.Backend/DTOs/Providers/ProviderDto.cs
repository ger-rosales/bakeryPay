namespace BakeryPay.Backend.DTOs.Providers;

public class ProviderDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactFirstName { get; set; } = string.Empty;
    public string ContactLastName { get; set; } = string.Empty;
    public string ContactIdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}
