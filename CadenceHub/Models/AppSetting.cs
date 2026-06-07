using System;
using System.Collections.Generic;

namespace CadenceHub.Models;

public partial class AppSetting
{
    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime UpdatedAt { get; set; }
}
