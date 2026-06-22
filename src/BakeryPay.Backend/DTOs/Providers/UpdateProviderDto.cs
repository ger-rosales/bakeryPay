namespace BakeryPay.Backend.DTOs.Providers;

public class UpdateProviderDto
{
    public string TaxId { get; set; } = string.Empty;
    public string ContactFirstName { get; set; } = string.Empty;
    public string ContactLastName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
}
