using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class VAttendanceMonthlyStaffSummary
{
    public string? ReportMonth { get; set; }

    public string StaffCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public int? PresentDays { get; set; }

    public int? AbsentDays { get; set; }

    public int? RecordedDays { get; set; }

    public decimal? AttendanceRatePercent { get; set; }
}
