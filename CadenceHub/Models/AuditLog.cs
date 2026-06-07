using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class AuditLog
{
    public int Id { get; set; }

    public int? ActorUserId { get; set; }

    public string ActionCode { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public int? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual UserAccount? ActorUser { get; set; }
}
