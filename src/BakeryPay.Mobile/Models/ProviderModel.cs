namespace BakeryPay.Mobile.Models;

public class ProviderModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactIdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;

    public string Initials
    {
        get
        {
            var parts = BusinessName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(x => char.ToUpperInvariant(x[0]));

            var result = string.Concat(parts);
            return string.IsNullOrWhiteSpace(result) ? "PR" : result;
        }
    }
}
