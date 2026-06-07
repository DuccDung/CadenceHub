using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class DutySchedule
{
    public int Id { get; set; }

    public DateOnly DutyDate { get; set; }

    public string ShiftCode { get; set; } = null!;

    public int StaffId { get; set; }

    public int? AssignedByUserId { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual UserAccount? AssignedByUser { get; set; }

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual Staff Staff { get; set; } = null!;
}
