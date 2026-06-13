using CadenceHub.Models;
using CadenceHub.Security;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace CadenceHub.Services;

public sealed class ExcelExportService
{
    private readonly BusinessDataService _dataService = new();
    private static readonly IReadOnlyDictionary<string, string> VietnameseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "ID",
        ["Ngay"] = "Ngày",
        ["Date"] = "Ngày",
        ["Month"] = "Tháng",
        ["StaffCode"] = "Mã cán bộ",
        ["FullName"] = "Họ tên",
        ["Unit"] = "Đơn vị",
        ["PositionCode"] = "Mã chức vụ",
        ["PositionName"] = "Chức vụ",
        ["StatusName"] = "Trạng thái",
        ["Count"] = "Số lượng",
        ["Rate"] = "Tỷ lệ (%)",
        ["Note"] = "Ghi chú",
        ["EnteredBy"] = "Người nhập",
        ["CreatedAt"] = "Thời gian tạo",
        ["UpdatedAt"] = "Thời gian cập nhật",
        ["PresentDays"] = "Số ngày có mặt",
        ["AbsentDays"] = "Số ngày vắng",
        ["RecordedDays"] = "Số ngày ghi nhận",
        ["AttendanceRate"] = "Tỷ lệ chuyên cần (%)",
        ["Username"] = "Tên đăng nhập",
        ["DisplayName"] = "Tên hiển thị",
        ["StaffId"] = "ID cán bộ",
        ["IsActive"] = "Hoạt động",
        ["LastLoginAt"] = "Lần đăng nhập gần nhất",
        ["UserId"] = "ID tài khoản",
        ["RoleId"] = "ID vai trò",
        ["Code"] = "Mã",
        ["Name"] = "Tên",
        ["Description"] = "Mô tả",
        ["SortOrder"] = "Thứ tự",
        ["IsPresentGroup"] = "Nhóm có mặt",
        ["IsAbsentGroup"] = "Nhóm vắng",
        ["DutyDate"] = "Ngày trực",
        ["ShiftCode"] = "Ca trực",
        ["AssignedByUserId"] = "ID người phân công",
        ["AttendanceDate"] = "Ngày điểm danh",
        ["StatusId"] = "ID trạng thái",
        ["EnteredByUserId"] = "ID người nhập",
        ["UpdatedByUserId"] = "ID người cập nhật",
        ["DutyScheduleId"] = "ID lịch trực",
        ["Key"] = "Mã cấu hình",
        ["Value"] = "Giá trị",
        ["ActorUserId"] = "ID người thao tác",
        ["ActionCode"] = "Hành động",
        ["EntityName"] = "Bảng dữ liệu",
        ["EntityId"] = "ID dữ liệu",
        ["OldValue"] = "Giá trị cũ",
        ["NewValue"] = "Giá trị mới"
    };

    public async Task<string> ExportDailyReportAsync(DateOnly date, string? outputPath = null)
    {
        var summary = await _dataService.GetDailySummaryAsync(date);
        var details = await _dataService.GetDailyDetailsAsync(date);
        var path = outputPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            var directory = await _dataService.GetExportDirectoryAsync();
            path = Path.Combine(directory, $"bao_cao_ngay_{date:yyyyMMdd}.xlsx");
        }

        using var workbook = new XLWorkbook();
        AddWorksheet(workbook, "Tổng hợp", summary);
        AddWorksheet(workbook, "Chi tiết", details.Select(row => new
        {
            Ngay = row.Date.ToString("dd/MM/yyyy"),
            row.StaffCode,
            row.FullName,
            row.Unit,
            row.PositionName,
            row.StatusName,
            row.Note,
            row.EnteredBy,
            row.CreatedAt,
            row.UpdatedAt
        }).ToList());

        EnsureOutputDirectory(path);
        workbook.SaveAs(path);
        return path;
    }

    public async Task<string> ExportMonthlyReportAsync(int year, int month, string? outputPath = null)
    {
        var rows = await _dataService.GetMonthlySummaryAsync(year, month);
        var path = outputPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            var directory = await _dataService.GetExportDirectoryAsync();
            path = Path.Combine(directory, $"bao_cao_thang_{year:0000}_{month:00}.xlsx");
        }

        using var workbook = new XLWorkbook();
        AddWorksheet(workbook, "Báo cáo tháng", rows);
        EnsureOutputDirectory(path);
        workbook.SaveAs(path);
        return path;
    }

    public async Task<string> CreateFullBackupWorkbookAsync(AuthenticatedUser actor)
    {
        var directory = await _dataService.GetBackupDirectoryAsync();
        var path = Path.Combine(directory, $"cadencehub_backup_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

        await using var db = new CadenceHubContext();
        using var workbook = new XLWorkbook();

        AddWorksheet(workbook, "staff", await db.Staff.AsNoTracking().OrderBy(item => item.StaffCode).Select(item => new
        {
            item.Id,
            item.StaffCode,
            item.FullName,
            item.Unit,
            item.PositionCode,
            item.PositionName,
            item.IsActive,
            item.CreatedAt,
            item.UpdatedAt
        }).ToListAsync());
        AddWorksheet(workbook, "roles", await db.Roles.AsNoTracking().OrderBy(item => item.Code).Select(item => new
        {
            item.Id,
            item.Code,
            item.Name,
            item.Description,
            item.CreatedAt
        }).ToListAsync());
        AddWorksheet(workbook, "user_accounts", await db.UserAccounts.AsNoTracking().OrderBy(item => item.Username).Select(item => new
        {
            item.Id,
            item.Username,
            item.DisplayName,
            item.StaffId,
            item.IsActive,
            item.LastLoginAt,
            item.CreatedAt,
            item.UpdatedAt
        }).ToListAsync());
        AddWorksheet(workbook, "user_roles", await db.UserRoles.AsNoTracking().Select(item => new
        {
            item.UserId,
            item.RoleId,
            item.CreatedAt
        }).ToListAsync());
        AddWorksheet(workbook, "attendance_statuses", await db.AttendanceStatuses.AsNoTracking().OrderBy(item => item.SortOrder).Select(item => new
        {
            item.Id,
            item.Code,
            item.Name,
            item.SortOrder,
            item.IsPresentGroup,
            item.IsAbsentGroup,
            item.IsActive
        }).ToListAsync());
        AddWorksheet(workbook, "duty_schedules", await db.DutySchedules.AsNoTracking().OrderBy(item => item.DutyDate).Select(item => new
        {
            item.Id,
            DutyDate = item.DutyDate.ToString("yyyy-MM-dd"),
            item.ShiftCode,
            item.StaffId,
            item.AssignedByUserId,
            item.Note,
            item.CreatedAt,
            item.UpdatedAt
        }).ToListAsync());
        AddWorksheet(workbook, "attendance_records", await db.AttendanceRecords.AsNoTracking().OrderBy(item => item.AttendanceDate).ThenBy(item => item.StaffId).Select(item => new
        {
            item.Id,
            AttendanceDate = item.AttendanceDate.ToString("yyyy-MM-dd"),
            item.StaffId,
            item.StatusId,
            item.EnteredByUserId,
            item.DutyScheduleId,
            item.Note,
            item.CreatedAt,
            item.UpdatedAt,
            item.UpdatedByUserId
        }).ToListAsync());
        AddWorksheet(workbook, "app_settings", await db.AppSettings.AsNoTracking().OrderBy(item => item.Key).Select(item => new
        {
            item.Key,
            item.Value,
            item.Description,
            item.UpdatedAt
        }).ToListAsync());
        AddWorksheet(workbook, "audit_logs", await db.AuditLogs.AsNoTracking().OrderByDescending(item => item.CreatedAt).Take(2000).Select(item => new
        {
            item.Id,
            item.ActorUserId,
            item.ActionCode,
            item.EntityName,
            item.EntityId,
            item.OldValue,
            item.NewValue,
            item.Note,
            item.CreatedAt
        }).ToListAsync());

        workbook.SaveAs(path);

        db.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actor.Id,
            ActionCode = "BACKUP_EXPORTED",
            EntityName = "database",
            NewValue = path,
            Note = "Xuất bản sao dữ liệu dạng Excel."
        });
        await db.SaveChangesAsync();

        return path;
    }

    private static void AddWorksheet<T>(XLWorkbook workbook, string sheetName, IReadOnlyCollection<T> rows)
    {
        var worksheet = workbook.Worksheets.Add(SanitizeSheetName(sheetName));
        if (rows.Count == 0)
        {
            worksheet.Cell(1, 1).Value = "Không có dữ liệu";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents(12, 70);
            return;
        }

        worksheet.Cell(1, 1).InsertTable(rows);
        ApplyVietnameseHeaders(worksheet);
        worksheet.Columns().AdjustToContents(12, 70);
        foreach (var column in worksheet.ColumnsUsed())
        {
            if (column.Width < 16)
            {
                column.Width = 16;
            }
        }

        worksheet.SheetView.FreezeRows(1);

        var used = worksheet.RangeUsed();
        if (used is not null)
        {
            used.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            used.Style.Alignment.WrapText = false;
            used.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            used.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#8A1218");
            worksheet.Row(1).Style.Font.FontColor = XLColor.White;
        }
    }

    private static string SanitizeSheetName(string value)
    {
        var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var sanitized = invalidChars.Aggregate(value, (current, ch) => current.Replace(ch, '_'));
        return sanitized.Length > 31 ? sanitized[..31] : sanitized;
    }

    private static void ApplyVietnameseHeaders(IXLWorksheet worksheet)
    {
        foreach (var cell in worksheet.Row(1).CellsUsed())
        {
            var key = cell.GetString();
            if (VietnameseHeaders.TryGetValue(key, out var vietnameseHeader))
            {
                cell.Value = vietnameseHeader;
            }
        }
    }

    private static void EnsureOutputDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
