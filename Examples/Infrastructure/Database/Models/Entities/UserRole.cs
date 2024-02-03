using Infrastructure.Database.Models.Base;

namespace Infrastructure.Database.Models.Entities;

public class UserRole : HasId
{

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }
}