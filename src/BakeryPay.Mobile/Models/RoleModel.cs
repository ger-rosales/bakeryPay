namespace BakeryPay.Mobile.Models;

public class RoleModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public int AssignedUsersCount { get; set; }

    public string Initials
    {
        get
        {
            var parts = Name
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(x => char.ToUpperInvariant(x[0]));

            var result = string.Concat(parts);
            return string.IsNullOrWhiteSpace(result) ? "RL" : result;
        }
    }
}
