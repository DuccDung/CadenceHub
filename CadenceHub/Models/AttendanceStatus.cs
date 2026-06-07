using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class AttendanceStatus
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsPresentGroup { get; set; }

    public bool IsAbsentGroup { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
