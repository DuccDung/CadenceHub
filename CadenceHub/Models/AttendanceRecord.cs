using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class AttendanceRecord
{
    public int Id { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public int StaffId { get; set; }

    public int StatusId { get; set; }

    public int EnteredByUserId { get; set; }

    public int? DutyScheduleId { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public virtual DutySchedule? DutySchedule { get; set; }

    public virtual UserAccount EnteredByUser { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;

    public virtual AttendanceStatus Status { get; set; } = null!;

    public virtual UserAccount? UpdatedByUser { get; set; }
}
