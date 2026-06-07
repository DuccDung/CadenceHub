using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class UserAccount
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int? StaffId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AttendanceRecord> AttendanceRecordEnteredByUsers { get; set; } = new List<AttendanceRecord>();

    public virtual ICollection<AttendanceRecord> AttendanceRecordUpdatedByUsers { get; set; } = new List<AttendanceRecord>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<DutySchedule> DutySchedules { get; set; } = new List<DutySchedule>();

    public virtual Staff? Staff { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
