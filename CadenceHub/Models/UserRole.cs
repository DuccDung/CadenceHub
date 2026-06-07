using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class UserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
