namespace BakeryPay.Backend.DTOs.Users;

public class UserDto
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
}
