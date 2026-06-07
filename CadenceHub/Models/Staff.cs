using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class Staff
{
    public int Id { get; set; }

    public string StaffCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public string? PositionCode { get; set; }

    public string? PositionName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual ICollection<DutySchedule> DutySchedules { get; set; } = new List<DutySchedule>();

    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}
