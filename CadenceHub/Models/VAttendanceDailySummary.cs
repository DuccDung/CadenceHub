using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class VAttendanceDailySummary
{
    public DateOnly AttendanceDate { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public int? TotalCount { get; set; }
}
