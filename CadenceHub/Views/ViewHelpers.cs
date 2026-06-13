using CadenceHub.UI;

namespace CadenceHub.Views;

public static class ViewHelpers
{
    private static readonly IReadOnlyDictionary<string, string> VietnameseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Id"] = "ID",
        ["StatusName"] = "Trạng thái",
        ["Count"] = "Số lượng",
        ["Rate"] = "Tỷ lệ (%)",
        ["Date"] = "Ngày",
        ["StaffCode"] = "Mã cán bộ",
        ["FullName"] = "Họ tên",
        ["Unit"] = "Đơn vị",
        ["PositionCode"] = "Mã chức vụ",
        ["PositionName"] = "Chức vụ",
        ["Note"] = "Ghi chú",
        ["EnteredBy"] = "Người nhập",
        ["CreatedAt"] = "Thời gian tạo",
        ["UpdatedAt"] = "Thời gian cập nhật",
        ["Month"] = "Tháng",
        ["PresentDays"] = "Số ngày có mặt",
        ["AbsentDays"] = "Số ngày vắng",
        ["RecordedDays"] = "Số ngày ghi nhận",
        ["AttendanceRate"] = "Tỷ lệ chuyên cần (%)",
        ["StaffId"] = "ID cán bộ",
        ["StaffName"] = "Cán bộ",
        ["AssignedBy"] = "Người phân công",
        ["DutyDate"] = "Ngày trực",
        ["ShiftCode"] = "Ca trực",
        ["Username"] = "Tài khoản",
        ["DisplayName"] = "Tên hiển thị",
        ["IsActive"] = "Hoạt động",
        ["Roles"] = "Vai trò",
        ["Key"] = "Mã cấu hình",
        ["Value"] = "Giá trị",
        ["Description"] = "Mô tả",
        ["Actor"] = "Người thao tác",
        ["ActionCode"] = "Hành động",
        ["EntityName"] = "Bảng dữ liệu",
        ["EntityId"] = "ID dữ liệu"
    };

    public static Label Title(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = AppTheme.Font(16, FontStyle.Bold),
            ForeColor = AppTheme.Navy,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static Button CommandButton(string text, Color? backColor = null)
    {
        var button = new Button
        {
            Text = text,
            Height = 40,
            MinimumSize = new Size(124, 40),
            AutoSize = true,
            Padding = new Padding(16, 0, 16, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor ?? AppTheme.PoliceRed,
            ForeColor = Color.White,
            Font = AppTheme.Font(10.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    public static void SetButtonState(Button button, bool enabled, Color? activeBackColor = null)
    {
        button.Enabled = enabled;
        button.BackColor = enabled ? activeBackColor ?? AppTheme.PoliceRed : Color.FromArgb(202, 207, 214);
        button.ForeColor = enabled ? Color.White : Color.FromArgb(111, 119, 130);
        button.Cursor = enabled ? Cursors.Hand : Cursors.Default;
    }

    public static void ClearGridSelection(DataGridView grid)
    {
        grid.ClearSelection();
        if (grid.Rows.Count > 0)
        {
            grid.CurrentCell = null;
        }
    }

    public static Label InfoLabel(string text, Color? color = null)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = AppTheme.Font(10.5f, FontStyle.Bold),
            ForeColor = color ?? AppTheme.MutedText,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static RoundedPanel Card(Padding? padding = null)
    {
        return new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            Padding = padding ?? new Padding(18)
        };
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = AppTheme.Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.Font = AppTheme.Font(10);
        grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.PoliceRed;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = AppTheme.Font(9.5f, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 52;
        grid.RowTemplate.Height = 34;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 244, 209);
        grid.DefaultCellStyle.SelectionForeColor = AppTheme.Ink;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        grid.DataBindingComplete += (_, _) => ApplyVietnameseHeaders(grid);
    }

    public static void ApplyVietnameseHeaders(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            var key = string.IsNullOrWhiteSpace(column.DataPropertyName) ? column.Name : column.DataPropertyName;
            if (VietnameseHeaders.TryGetValue(key, out var header))
            {
                column.HeaderText = header;
            }

            if (key.EndsWith("At", StringComparison.OrdinalIgnoreCase))
            {
                column.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
            else if (string.Equals(key, "Date", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(key, "DutyDate", StringComparison.OrdinalIgnoreCase))
            {
                column.DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            else if (key.Contains("Rate", StringComparison.OrdinalIgnoreCase))
            {
                column.DefaultCellStyle.Format = "0.##";
            }
        }
    }

    public static DateOnly DateOnlyFrom(DateTimePicker picker)
    {
        return DateOnly.FromDateTime(picker.Value.Date);
    }

    public static string? PickExcelSavePath(IWin32Window owner, string fileName)
    {
        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "xlsx",
            FileName = fileName,
            Filter = "Excel Workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            OverwritePrompt = true,
            Title = "Chon noi luu file Excel"
        };

        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
    }

    public static void ShowError(IWin32Window owner, Exception ex)
    {
        MessageBox.Show(owner, ex.Message, "Lỗi xử lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void ShowInfo(IWin32Window owner, string message)
    {
        MessageBox.Show(owner, message, "CadenceHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
