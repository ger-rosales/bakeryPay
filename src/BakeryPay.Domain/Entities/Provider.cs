using BakeryPay.Domain.Common;

namespace BakeryPay.Domain.Entities;

public class Provider : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactIdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
