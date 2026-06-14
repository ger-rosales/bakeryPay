using BakeryPay.Domain.Common;

namespace BakeryPay.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
