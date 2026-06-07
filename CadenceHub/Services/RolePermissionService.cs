using CadenceHub.Models;
using CadenceHub.Security;
using Microsoft.EntityFrameworkCore;

namespace CadenceHub.Services;

public sealed class RolePermissionService
{
    private static readonly IReadOnlyDictionary<string, Permission[]> RolePermissions =
        new Dictionary<string, Permission[]>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleCodes.Admin] =
            [
                Permission.ViewDashboard,
                Permission.TakeAttendance,
                Permission.ViewReports,
                Permission.ExportReports,
                Permission.ManageStaff,
                Permission.ManageUsers,
                Permission.ManageDutySchedule,
                Permission.ManageSettings,
                Permission.ViewAuditLogs,
                Permission.BackupRestore
            ],
            [RoleCodes.Leader] =
            [
                Permission.ViewDashboard,
                Permission.ViewReports,
                Permission.ExportReports
            ],
            [RoleCodes.DutyOfficer] =
            [
                Permission.ViewDashboard,
                Permission.TakeAttendance
            ],
            [RoleCodes.StaffViewer] =
            [
                Permission.ViewDashboard
            ]
        };

    public bool Can(AuthenticatedUser user, Permission permission)
    {
        return user.RoleCodes.Any(roleCode =>
            RolePermissions.TryGetValue(roleCode, out var permissions)
            && permissions.Contains(permission));
    }

    public bool HasAnyPermission(AuthenticatedUser user)
    {
        return user.RoleCodes.Any(roleCode => RolePermissions.ContainsKey(roleCode));
    }

    public async Task<bool> CanEditAttendanceTodayAsync(AuthenticatedUser user, DateTime today)
    {
        if (user.HasRole(RoleCodes.Admin))
        {
            return true;
        }

        if (!Can(user, Permission.TakeAttendance) || user.StaffId is null)
        {
            return false;
        }

        await using var db = new CadenceHubContext();
        var date = DateOnly.FromDateTime(today.Date);

        return await db.DutySchedules.AnyAsync(schedule =>
            schedule.StaffId == user.StaffId.Value
            && schedule.DutyDate == date
            && (schedule.ShiftCode == "FULL_DAY" || schedule.ShiftCode == "MORNING"));
    }
}
