using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class VAttendanceDailyDetail
{
    public DateOnly AttendanceDate { get; set; }

    public string StaffCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public string? PositionCode { get; set; }

    public string? PositionName { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public string? Note { get; set; }

    public string EnteredBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
