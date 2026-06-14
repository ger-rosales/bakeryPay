namespace BakeryPay.Mobile.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool BiometricEnabled { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsActive { get; set; }
    public Guid? ProviderId { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Initials
    {
        get
        {
            var source = string.IsNullOrWhiteSpace(FullName) ? Email : FullName;
            var parts = source
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(x => char.ToUpperInvariant(x[0]));

            var result = string.Concat(parts);
            return string.IsNullOrWhiteSpace(result) ? "US" : result;
        }
    }

    public string StatusDisplay => IsActive ? "Activo" : "Inactivo";

    public string StatusColor => IsActive ? "#1FA971" : "#D64545";
}
