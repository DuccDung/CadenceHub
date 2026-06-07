using CadenceHub.Models;
using CadenceHub.Security;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace CadenceHub.Services;

public sealed class BusinessDataService
{
    private readonly RolePermissionService _permissionService = new();

    public async Task<DashboardSnapshot> GetDashboardAsync(DateOnly date)
    {
        await using var db = new CadenceHubContext();

        var totalStaff = await db.Staff.CountAsync(staff => staff.IsActive);
        var records = await db.AttendanceRecords
            .Include(record => record.Status)
            .Where(record => record.AttendanceDate == date)
            .ToListAsync();

        var recorded = records.Select(record => record.StaffId).Distinct().Count();
        var present = records.Count(record => record.Status.IsPresentGroup);
        var absent = records.Count(record => record.Status.IsAbsentGroup);
        var statusSummary = records
            .GroupBy(record => record.Status.Name)
            .Select(group => new StatusSummaryItem(group.Key, group.Count()))
            .OrderBy(item => item.StatusName)
            .ToList();

        return new DashboardSnapshot(totalStaff, recorded, Math.Max(0, totalStaff - recorded), present, absent, statusSummary);
    }

    public async Task<List<AttendanceStatusOption>> GetAttendanceStatusesAsync()
    {
        await using var db = new CadenceHubContext();
        return await db.AttendanceStatuses
            .Where(status => status.IsActive)
            .OrderBy(status => status.SortOrder)
            .Select(status => new AttendanceStatusOption(status.Id, status.Name))
            .ToListAsync();
    }

    public async Task<AttendanceEditPolicy> GetAttendanceEditPolicyAsync(AuthenticatedUser user, DateOnly date)
    {
        if (!_permissionService.Can(user, Permission.TakeAttendance))
        {
            return new AttendanceEditPolicy(false, "Tài khoản không có quyền điểm danh.");
        }

        if (date != DateOnly.FromDateTime(DateTime.Today))
        {
            return new AttendanceEditPolicy(false, "Chỉ cho phép nhập/sửa điểm danh trong ngày hiện tại.");
        }

        await using var db = new CadenceHubContext();
        var lockTime = await GetSettingTimeAsync(db, "ATTENDANCE_LOCK_TIME", new TimeOnly(10, 0));
        var isLocked = TimeOnly.FromDateTime(DateTime.Now) > lockTime;
        var allowAdminAfterLock = await GetSettingBoolAsync(db, "ATTENDANCE_ALLOW_ADMIN_EDIT_AFTER_LOCK", true);

        if (user.HasRole(RoleCodes.Admin))
        {
            if (isLocked && !allowAdminAfterLock)
            {
                return new AttendanceEditPolicy(false, $"Đã quá giờ khóa {lockTime:HH\\:mm}; cấu hình hiện không cho phép admin sửa.");
            }

            return new AttendanceEditPolicy(true, isLocked ? $"Admin đang sửa sau giờ khóa {lockTime:HH\\:mm}." : "Admin có toàn quyền điểm danh.");
        }

        if (isLocked)
        {
            return new AttendanceEditPolicy(false, $"Đã quá giờ khóa {lockTime:HH\\:mm}. Chỉ quản trị được xử lý ngoại lệ.");
        }

        if (user.StaffId is null)
        {
            return new AttendanceEditPolicy(false, "Tài khoản trực ban chưa liên kết với cán bộ.");
        }

        var hasDuty = await db.DutySchedules.AnyAsync(schedule =>
            schedule.DutyDate == date
            && schedule.StaffId == user.StaffId.Value
            && (schedule.ShiftCode == "FULL_DAY" || schedule.ShiftCode == "MORNING"));

        return hasDuty
            ? new AttendanceEditPolicy(true, "Được phép điểm danh theo lịch trực hôm nay.")
            : new AttendanceEditPolicy(false, "Cán bộ chưa được phân công trực ban sáng nay.");
    }

    public async Task<List<AttendanceEntryRow>> GetAttendanceEntriesAsync(DateOnly date)
    {
        await using var db = new CadenceHubContext();

        var records = await db.AttendanceRecords
            .Where(record => record.AttendanceDate == date)
            .ToDictionaryAsync(record => record.StaffId);

        var staff = await db.Staff
            .Where(item => item.IsActive)
            .OrderBy(item => item.StaffCode)
            .ToListAsync();

        return staff.Select(item =>
        {
            records.TryGetValue(item.Id, out var record);
            return new AttendanceEntryRow
            {
                StaffId = item.Id,
                StaffCode = item.StaffCode,
                FullName = item.FullName,
                Unit = item.Unit,
                PositionName = item.PositionName ?? item.PositionCode ?? string.Empty,
                StatusId = record?.StatusId,
                Note = record?.Note ?? string.Empty
            };
        }).ToList();
    }

    public async Task SaveAttendanceAsync(DateOnly date, IEnumerable<AttendanceEntryRow> rows, AuthenticatedUser user)
    {
        await using var db = new CadenceHubContext();
        var staffIds = rows.Select(row => row.StaffId).Distinct().ToArray();
        var existingRecords = await db.AttendanceRecords
            .Where(record => record.AttendanceDate == date && staffIds.Contains(record.StaffId))
            .ToDictionaryAsync(record => record.StaffId);

        var now = DateTime.Now;
        foreach (var row in rows)
        {
            if (row.StatusId is not int statusId)
            {
                continue;
            }

            if (existingRecords.TryGetValue(row.StaffId, out var existing))
            {
                var oldValue = $"status_id={existing.StatusId};note={existing.Note}";
                existing.StatusId = statusId;
                existing.Note = NormalizeNullable(row.Note);
                existing.UpdatedAt = now;
                existing.UpdatedByUserId = user.Id;
                db.AuditLogs.Add(BuildAudit(user.Id, "ATTENDANCE_UPDATED", "attendance_records", existing.Id, oldValue, $"status_id={existing.StatusId};note={existing.Note}", "Cập nhật điểm danh."));
            }
            else
            {
                var created = new AttendanceRecord
                {
                    AttendanceDate = date,
                    StaffId = row.StaffId,
                    StatusId = statusId,
                    EnteredByUserId = user.Id,
                    Note = NormalizeNullable(row.Note)
                };
                db.AttendanceRecords.Add(created);
                db.AuditLogs.Add(BuildAudit(user.Id, "ATTENDANCE_CREATED", "attendance_records", null, null, $"date={date};staff_id={row.StaffId};status_id={row.StatusId}", "Tạo điểm danh."));
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<DailySummaryRow>> GetDailySummaryAsync(DateOnly date)
    {
        await using var db = new CadenceHubContext();
        var totalStaff = await db.Staff.CountAsync(staff => staff.IsActive);
        var statuses = await db.AttendanceStatuses
            .Where(status => status.IsActive)
            .OrderBy(status => status.SortOrder)
            .ToListAsync();
        var counts = await db.AttendanceRecords
            .Where(record => record.AttendanceDate == date)
            .GroupBy(record => record.StatusId)
            .Select(group => new { StatusId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.StatusId, item => item.Count);

        return statuses.Select(status =>
        {
            counts.TryGetValue(status.Id, out var count);
            return new DailySummaryRow
            {
                StatusName = status.Name,
                Count = count,
                Rate = totalStaff == 0 ? 0 : Math.Round(count * 100m / totalStaff, 2)
            };
        }).ToList();
    }

    public async Task<List<DailyDetailRow>> GetDailyDetailsAsync(DateOnly date)
    {
        await using var db = new CadenceHubContext();
        var rows = await db.AttendanceRecords
            .Include(record => record.Staff)
            .Include(record => record.Status)
            .Include(record => record.EnteredByUser)
            .Where(record => record.AttendanceDate == date)
            .OrderBy(record => record.Staff.StaffCode)
            .Select(record => new DailyDetailRow
            {
                Date = record.AttendanceDate,
                StaffCode = record.Staff.StaffCode,
                FullName = record.Staff.FullName,
                Unit = record.Staff.Unit,
                PositionName = record.Staff.PositionName ?? record.Staff.PositionCode ?? string.Empty,
                StatusName = record.Status.Name,
                Note = record.Note ?? string.Empty,
                EnteredBy = record.EnteredByUser.Username,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            })
            .ToListAsync();

        return rows;
    }

    public async Task<List<MonthlyStaffSummaryRow>> GetMonthlySummaryAsync(int year, int month)
    {
        await using var db = new CadenceHubContext();
        var fromDate = new DateOnly(year, month, 1);
        var toDate = fromDate.AddMonths(1);

        var records = await db.AttendanceRecords
            .Include(record => record.Staff)
            .Include(record => record.Status)
            .Where(record => record.AttendanceDate >= fromDate && record.AttendanceDate < toDate)
            .ToListAsync();

        var activeStaff = await db.Staff
            .Where(staff => staff.IsActive)
            .OrderBy(staff => staff.StaffCode)
            .ToListAsync();

        return activeStaff.Select(staff =>
        {
            var staffRecords = records.Where(record => record.StaffId == staff.Id).ToList();
            var present = staffRecords.Count(record => record.Status.IsPresentGroup);
            var absent = staffRecords.Count(record => record.Status.IsAbsentGroup);
            var recorded = staffRecords.Count;
            return new MonthlyStaffSummaryRow
            {
                Month = $"{year:0000}-{month:00}",
                StaffCode = staff.StaffCode,
                FullName = staff.FullName,
                Unit = staff.Unit,
                PresentDays = present,
                AbsentDays = absent,
                RecordedDays = recorded,
                AttendanceRate = recorded == 0 ? 0 : Math.Round(present * 100m / recorded, 2)
            };
        }).ToList();
    }

    public async Task<List<StaffEditorRow>> GetStaffRowsAsync(bool includeInactive = true)
    {
        await using var db = new CadenceHubContext();
        return await db.Staff
            .Where(staff => includeInactive || staff.IsActive)
            .OrderBy(staff => staff.StaffCode)
            .Select(staff => new StaffEditorRow
            {
                Id = staff.Id,
                StaffCode = staff.StaffCode,
                FullName = staff.FullName,
                Unit = staff.Unit,
                PositionCode = staff.PositionCode ?? string.Empty,
                PositionName = staff.PositionName ?? string.Empty,
                IsActive = staff.IsActive
            })
            .ToListAsync();
    }

    public async Task SaveStaffAsync(StaffEditorRow row, AuthenticatedUser user)
    {
        await using var db = new CadenceHubContext();

        var normalizedCode = row.StaffCode.Trim();
        var duplicateExists = await db.Staff.AnyAsync(staff => staff.StaffCode == normalizedCode && staff.Id != row.Id);
        if (duplicateExists)
        {
            throw new InvalidOperationException("Mã cán bộ đã tồn tại.");
        }

        if (row.Id == 0)
        {
            var staff = new Staff
            {
                StaffCode = normalizedCode,
                FullName = row.FullName.Trim(),
                Unit = row.Unit.Trim(),
                PositionCode = NormalizeNullable(row.PositionCode),
                PositionName = NormalizeNullable(row.PositionName),
                IsActive = row.IsActive
            };
            db.Staff.Add(staff);
            await db.SaveChangesAsync();
            db.AuditLogs.Add(BuildAudit(user.Id, "STAFF_CREATED", "staff", staff.Id, null, staff.StaffCode, "Tạo cán bộ."));
        }
        else
        {
            var staff = await db.Staff.SingleAsync(item => item.Id == row.Id);
            var oldValue = $"{staff.StaffCode};{staff.FullName};{staff.Unit};{staff.IsActive}";
            staff.StaffCode = normalizedCode;
            staff.FullName = row.FullName.Trim();
            staff.Unit = row.Unit.Trim();
            staff.PositionCode = NormalizeNullable(row.PositionCode);
            staff.PositionName = NormalizeNullable(row.PositionName);
            staff.IsActive = row.IsActive;
            staff.UpdatedAt = DateTime.Now;
            db.AuditLogs.Add(BuildAudit(user.Id, "STAFF_UPDATED", "staff", staff.Id, oldValue, $"{staff.StaffCode};{staff.FullName};{staff.Unit};{staff.IsActive}", "Cập nhật cán bộ."));
        }

        await db.SaveChangesAsync();
    }

    public async Task ImportStaffFromExcelAsync(string filePath, AuthenticatedUser user)
    {
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.RowsUsed().First();
        var headers = headerRow.Cells()
            .Select(cell => new { Name = cell.GetString().Trim(), Column = cell.Address.ColumnNumber })
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Column, StringComparer.OrdinalIgnoreCase);

        var nameCol = ResolveColumn(headers, "HoTen", "Họ tên", "Ho ten");
        var unitCol = ResolveColumn(headers, "DonVi", "Đơn vị", "Don vi");
        var positionCodeCol = ResolveColumn(headers, "ChucVu", "Chức vụ", "Chuc vu");
        var positionNameCol = headers.TryGetValue("Column1", out var col) ? col : positionCodeCol;

        await using var db = new CadenceHubContext();
        var created = 0;
        var updated = 0;

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var fullName = row.Cell(nameCol).GetString().Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                continue;
            }

            var unit = row.Cell(unitCol).GetString().Trim();
            var positionCode = row.Cell(positionCodeCol).GetString().Trim();
            var positionName = row.Cell(positionNameCol).GetString().Trim();
            var existing = await db.Staff.FirstOrDefaultAsync(staff => staff.FullName == fullName && staff.Unit == unit);

            if (existing is null)
            {
                var nextNumber = await db.Staff.CountAsync() + created + 1;
                db.Staff.Add(new Staff
                {
                    StaffCode = $"CB{nextNumber:000}",
                    FullName = fullName,
                    Unit = string.IsNullOrWhiteSpace(unit) ? "NVĐT" : unit,
                    PositionCode = NormalizeNullable(positionCode),
                    PositionName = NormalizeNullable(positionName),
                    IsActive = true
                });
                created++;
            }
            else
            {
                existing.PositionCode = NormalizeNullable(positionCode);
                existing.PositionName = NormalizeNullable(positionName);
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.Now;
                updated++;
            }
        }

        db.AuditLogs.Add(BuildAudit(user.Id, "STAFF_IMPORTED", "staff", null, null, $"created={created};updated={updated}", Path.GetFileName(filePath)));
        await db.SaveChangesAsync();
    }

    public async Task<List<RoleOption>> GetRoleOptionsAsync()
    {
        await using var db = new CadenceHubContext();
        return await db.Roles
            .OrderBy(role => role.Name)
            .Select(role => new RoleOption(role.Id, role.Code, role.Name))
            .ToListAsync();
    }

    public async Task<List<StaffOption>> GetStaffOptionsAsync(bool activeOnly = true)
    {
        await using var db = new CadenceHubContext();
        return await db.Staff
            .Where(staff => !activeOnly || staff.IsActive)
            .OrderBy(staff => staff.FullName)
            .Select(staff => new StaffOption(staff.Id, $"{staff.StaffCode} - {staff.FullName}"))
            .ToListAsync();
    }

    public async Task<List<LinkedStaffOption>> GetLinkedStaffOptionsAsync()
    {
        await using var db = new CadenceHubContext();
        var linkedUsers = await db.UserAccounts
            .Where(user => user.IsActive && user.StaffId != null && user.Staff != null && user.Staff.IsActive)
            .OrderBy(user => user.Staff!.FullName)
            .ThenBy(user => user.Username)
            .Select(user => new
            {
                StaffId = user.StaffId!.Value,
                user.Username,
                StaffCode = user.Staff!.StaffCode,
                FullName = user.Staff.FullName,
                user.Staff.Unit
            })
            .ToListAsync();

        return linkedUsers
            .GroupBy(item => new { item.StaffId, item.StaffCode, item.FullName, item.Unit })
            .OrderBy(group => group.Key.FullName)
            .ThenBy(group => group.Key.StaffCode)
            .Select(group => new LinkedStaffOption(
                group.Key.StaffId,
                group.Key.StaffCode,
                group.Key.FullName,
                group.Key.Unit,
                string.Join(", ", group.Select(item => item.Username).Distinct(StringComparer.OrdinalIgnoreCase))))
            .ToList();
    }

    public async Task<List<UserEditorRow>> GetUserRowsAsync()
    {
        await using var db = new CadenceHubContext();
        return await db.UserAccounts
            .Include(user => user.Staff)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.Username)
            .Select(user => new UserEditorRow
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                StaffId = user.StaffId,
                StaffName = user.Staff == null ? string.Empty : user.Staff.FullName,
                IsActive = user.IsActive,
                Roles = string.Join(", ", user.UserRoles.Select(userRole => userRole.Role.Code))
            })
            .ToListAsync();
    }

    public async Task SaveUserAsync(UserEditorInput input, AuthenticatedUser actor)
    {
        await using var db = new CadenceHubContext();
        var username = input.Username.Trim();
        var duplicateExists = await db.UserAccounts.AnyAsync(user => user.Username == username && user.Id != input.Id);
        if (duplicateExists)
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
        }

        UserAccount userAccount;
        if (input.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(input.NewPassword))
            {
                throw new InvalidOperationException("Tài khoản mới bắt buộc có mật khẩu.");
            }

            userAccount = new UserAccount
            {
                Username = username,
                DisplayName = input.DisplayName.Trim(),
                PasswordHash = PasswordHasher.Hash(input.NewPassword),
                StaffId = input.StaffId,
                IsActive = input.IsActive
            };
            db.UserAccounts.Add(userAccount);
            await db.SaveChangesAsync();
            db.AuditLogs.Add(BuildAudit(actor.Id, "USER_CREATED", "user_accounts", userAccount.Id, null, userAccount.Username, "Tạo tài khoản."));
        }
        else
        {
            userAccount = await db.UserAccounts.SingleAsync(user => user.Id == input.Id);
            userAccount.Username = username;
            userAccount.DisplayName = input.DisplayName.Trim();
            userAccount.StaffId = input.StaffId;
            userAccount.IsActive = input.IsActive;
            userAccount.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(input.NewPassword))
            {
                userAccount.PasswordHash = PasswordHasher.Hash(input.NewPassword);
            }
            db.AuditLogs.Add(BuildAudit(actor.Id, "USER_UPDATED", "user_accounts", userAccount.Id, null, userAccount.Username, "Cập nhật tài khoản."));
        }

        var desiredRoleIds = input.RoleIds.Distinct().ToHashSet();
        var existingRoles = await db.UserRoles
            .Where(userRole => userRole.UserId == userAccount.Id)
            .ToListAsync();
        var rolesToRemove = existingRoles.Where(userRole => !desiredRoleIds.Contains(userRole.RoleId)).ToList();
        db.UserRoles.RemoveRange(rolesToRemove);

        var existingRoleIds = existingRoles.Select(userRole => userRole.RoleId).ToHashSet();
        foreach (var roleId in desiredRoleIds.Where(roleId => !existingRoleIds.Contains(roleId)))
        {
            db.UserRoles.Add(new UserRole { UserId = userAccount.Id, RoleId = roleId });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<DutyScheduleRow>> GetDutySchedulesAsync(DateOnly fromDate, DateOnly toDate)
    {
        await using var db = new CadenceHubContext();
        return await db.DutySchedules
            .Include(schedule => schedule.Staff)
            .Include(schedule => schedule.AssignedByUser)
            .Where(schedule => schedule.DutyDate >= fromDate && schedule.DutyDate <= toDate)
            .OrderBy(schedule => schedule.DutyDate)
            .ThenBy(schedule => schedule.ShiftCode)
            .Select(schedule => new DutyScheduleRow
            {
                Id = schedule.Id,
                DutyDate = schedule.DutyDate,
                ShiftCode = schedule.ShiftCode,
                StaffId = schedule.StaffId,
                StaffName = schedule.Staff.FullName,
                AssignedBy = schedule.AssignedByUser == null ? string.Empty : schedule.AssignedByUser.Username,
                Note = schedule.Note ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task SaveDutyScheduleAsync(DutyScheduleRow row, AuthenticatedUser actor)
    {
        await using var db = new CadenceHubContext();

        var canAssignStaff = await db.Staff.AnyAsync(staff =>
            staff.Id == row.StaffId
            && staff.IsActive
            && staff.UserAccounts.Any(user => user.IsActive));
        if (!canAssignStaff)
        {
            throw new InvalidOperationException("Chỉ được phân công cán bộ đang hoạt động và đã liên kết tài khoản đang hoạt động.");
        }

        var duplicateExists = await db.DutySchedules.AnyAsync(schedule =>
            schedule.DutyDate == row.DutyDate
            && schedule.ShiftCode == row.ShiftCode
            && schedule.StaffId == row.StaffId
            && schedule.Id != row.Id);
        if (duplicateExists)
        {
            throw new InvalidOperationException("Cán bộ này đã có lịch trực trùng ngày và ca.");
        }

        if (row.Id == 0)
        {
            var schedule = new DutySchedule
            {
                DutyDate = row.DutyDate,
                ShiftCode = row.ShiftCode,
                StaffId = row.StaffId,
                AssignedByUserId = actor.Id,
                Note = NormalizeNullable(row.Note)
            };
            db.DutySchedules.Add(schedule);
            await db.SaveChangesAsync();
            db.AuditLogs.Add(BuildAudit(actor.Id, "DUTY_CREATED", "duty_schedules", schedule.Id, null, $"{schedule.DutyDate};{schedule.ShiftCode};{schedule.StaffId}", "Tạo lịch trực."));
        }
        else
        {
            var schedule = await db.DutySchedules.SingleAsync(item => item.Id == row.Id);
            var oldValue = $"{schedule.DutyDate};{schedule.ShiftCode};{schedule.StaffId}";
            schedule.DutyDate = row.DutyDate;
            schedule.ShiftCode = row.ShiftCode;
            schedule.StaffId = row.StaffId;
            schedule.AssignedByUserId = actor.Id;
            schedule.Note = NormalizeNullable(row.Note);
            schedule.UpdatedAt = DateTime.Now;
            db.AuditLogs.Add(BuildAudit(actor.Id, "DUTY_UPDATED", "duty_schedules", schedule.Id, oldValue, $"{schedule.DutyDate};{schedule.ShiftCode};{schedule.StaffId}", "Cập nhật lịch trực."));
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteDutyScheduleAsync(int id, AuthenticatedUser actor)
    {
        await using var db = new CadenceHubContext();
        var schedule = await db.DutySchedules.SingleAsync(item => item.Id == id);
        db.DutySchedules.Remove(schedule);
        db.AuditLogs.Add(BuildAudit(actor.Id, "DUTY_DELETED", "duty_schedules", id, $"{schedule.DutyDate};{schedule.ShiftCode};{schedule.StaffId}", null, "Xóa lịch trực."));
        await db.SaveChangesAsync();
    }

    public async Task<List<SettingRow>> GetSettingsAsync()
    {
        await using var db = new CadenceHubContext();
        return await db.AppSettings
            .OrderBy(setting => setting.Key)
            .Select(setting => new SettingRow
            {
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description ?? string.Empty,
                UpdatedAt = setting.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task SaveSettingAsync(SettingRow row, AuthenticatedUser actor)
    {
        await using var db = new CadenceHubContext();
        var setting = await db.AppSettings.SingleAsync(item => item.Key == row.Key);
        var oldValue = setting.Value;
        setting.Value = row.Value.Trim();
        setting.UpdatedAt = DateTime.Now;
        db.AuditLogs.Add(BuildAudit(actor.Id, "SETTING_UPDATED", "app_settings", null, oldValue, setting.Value, setting.Key));
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLogRow>> GetAuditLogsAsync(DateOnly fromDate, DateOnly toDate)
    {
        await using var db = new CadenceHubContext();
        var from = fromDate.ToDateTime(TimeOnly.MinValue);
        var to = toDate.ToDateTime(TimeOnly.MaxValue);
        return await db.AuditLogs
            .Include(log => log.ActorUser)
            .Where(log => log.CreatedAt >= from && log.CreatedAt <= to)
            .OrderByDescending(log => log.CreatedAt)
            .Take(500)
            .Select(log => new AuditLogRow
            {
                CreatedAt = log.CreatedAt,
                Actor = log.ActorUser == null ? string.Empty : log.ActorUser.Username,
                ActionCode = log.ActionCode,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Note = log.Note ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<string> GetExportDirectoryAsync()
    {
        await using var db = new CadenceHubContext();
        var configured = await db.AppSettings
            .Where(setting => setting.Key == "EXPORT_DIRECTORY")
            .Select(setting => setting.Value)
            .FirstOrDefaultAsync();

        return ResolveWorkspaceDirectory(configured, "exports");
    }

    public async Task<string> GetBackupDirectoryAsync()
    {
        await using var db = new CadenceHubContext();
        var configured = await db.AppSettings
            .Where(setting => setting.Key == "BACKUP_DIRECTORY")
            .Select(setting => setting.Value)
            .FirstOrDefaultAsync();

        return ResolveWorkspaceDirectory(configured, "backups");
    }

    private static AuditLog BuildAudit(int actorUserId, string actionCode, string entityName, int? entityId, string? oldValue, string? newValue, string? note)
    {
        return new AuditLog
        {
            ActorUserId = actorUserId,
            ActionCode = actionCode,
            EntityName = entityName,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            Note = note
        };
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static async Task<TimeOnly> GetSettingTimeAsync(CadenceHubContext db, string key, TimeOnly fallback)
    {
        var value = await db.AppSettings.Where(setting => setting.Key == key).Select(setting => setting.Value).FirstOrDefaultAsync();
        return TimeOnly.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static async Task<bool> GetSettingBoolAsync(CadenceHubContext db, string key, bool fallback)
    {
        var value = await db.AppSettings.Where(setting => setting.Key == key).Select(setting => setting.Value).FirstOrDefaultAsync();
        return value switch
        {
            "1" => true,
            "0" => false,
            _ when bool.TryParse(value, out var parsed) => parsed,
            _ => fallback
        };
    }

    private static int ResolveColumn(IReadOnlyDictionary<string, int> headers, params string[] names)
    {
        foreach (var name in names)
        {
            if (headers.TryGetValue(name, out var index))
            {
                return index;
            }
        }

        throw new InvalidOperationException($"Không tìm thấy cột: {string.Join(", ", names)}");
    }

    private static string ResolveWorkspaceDirectory(string? configuredPath, string fallbackDirectoryName)
    {
        var path = string.IsNullOrWhiteSpace(configuredPath) ? fallbackDirectoryName : configuredPath.Trim();
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, path);
        }

        Directory.CreateDirectory(path);
        return path;
    }
}

public sealed record DashboardSnapshot(int TotalStaff, int RecordedCount, int NotRecordedCount, int PresentCount, int AbsentCount, IReadOnlyList<StatusSummaryItem> StatusSummary);

public sealed record StatusSummaryItem(string StatusName, int Count);

public sealed record AttendanceStatusOption(int Id, string Name);

public sealed record RoleOption(int Id, string Code, string Name);

public sealed record StaffOption(int Id, string Name);

public sealed record LinkedStaffOption(int Id, string StaffCode, string FullName, string Unit, string Usernames)
{
    public string DisplayName => $"{StaffCode} - {FullName}";
}

public sealed record AttendanceEditPolicy(bool CanEdit, string Message);

public sealed class AttendanceEntryRow
{
    public int StaffId { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int? StatusId { get; set; }
    public string Note { get; set; } = string.Empty;
}

public sealed class DailySummaryRow
{
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Rate { get; set; }
}

public sealed class DailyDetailRow
{
    public DateOnly Date { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string EnteredBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class MonthlyStaffSummaryRow
{
    public string Month { get; set; } = string.Empty;
    public string StaffCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int RecordedDays { get; set; }
    public decimal AttendanceRate { get; set; }
}

public sealed class StaffEditorRow
{
    public int Id { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class UserEditorRow
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Roles { get; set; } = string.Empty;
}

public sealed class UserEditorInput
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int? StaffId { get; set; }
    public bool IsActive { get; set; } = true;
    public string NewPassword { get; set; } = string.Empty;
    public IReadOnlyCollection<int> RoleIds { get; set; } = Array.Empty<int>();
}

public sealed class DutyScheduleRow
{
    public int Id { get; set; }
    public DateOnly DutyDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string ShiftCode { get; set; } = "FULL_DAY";
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public sealed class SettingRow
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public sealed class AuditLogRow
{
    public DateTime CreatedAt { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string ActionCode { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Note { get; set; } = string.Empty;
}
