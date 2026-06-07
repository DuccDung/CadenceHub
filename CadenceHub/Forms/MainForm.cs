using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;
using CadenceHub.Views;
using System.ComponentModel;

namespace CadenceHub.Forms;

public sealed class MainForm : Form
{
    private readonly AuthenticatedUser _currentUser;
    private readonly RolePermissionService _permissionService = new();
    private readonly HiddenScrollFlowLayoutPanel _navigationPanel = new();
    private readonly Panel _contentPanel = new();
    private readonly Label _pageTitleLabel = new();
    private bool _canEditAttendanceToday;

    private static readonly Color NavigationNormalBack = AppTheme.PoliceRed;
    private static readonly Color NavigationActiveBack = Color.FromArgb(72, 8, 14);
    private static readonly Color NavigationDisabledBack = Color.FromArgb(98, 72, 76);

    public MainForm(AuthenticatedUser currentUser)
    {
        _currentUser = currentUser;

        Text = "CadenceHub - Quản lý điểm danh cán bộ";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 720);
        Size = new Size(1360, 820);
        BackColor = AppTheme.Page;
        Font = AppTheme.Font(10);
        AppIconProvider.ApplyTo(this);

        BuildLayout();
        Load += async (_, _) => await LoadPermissionsAsync();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool LogoutRequested { get; private set; }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Page
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 292));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildSidebar(), 0, 0);
        root.Controls.Add(BuildMainArea(), 1, 0);
        Controls.Add(root);
    }

    private Control BuildSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.PoliceRedDark,
            Padding = new Padding(20, 24, 20, 20)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = AppTheme.PoliceRedDark
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        var brand = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.PoliceRedDark
        };
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var logoBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LogoProvider.LoadLogo(),
            Margin = new Padding(0, 6, 12, 18)
        };
        brand.Controls.Add(logoBox, 0, 0);

        var brandText = new Label
        {
            Text = "CADENCEHUB\r\nĐIỂM DANH CÁN BỘ",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = AppTheme.Font(15, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        brand.Controls.Add(brandText, 1, 0);

        layout.Controls.Add(brand, 0, 0);

        var roleBadge = new Label
        {
            Text = string.Join(" • ", _currentUser.RoleNames),
            Dock = DockStyle.Fill,
            ForeColor = AppTheme.GoldSoft,
            Font = AppTheme.Font(9.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.TopLeft
        };
        layout.Controls.Add(roleBadge, 0, 1);

        _navigationPanel.Dock = DockStyle.Fill;
        _navigationPanel.BackColor = AppTheme.PoliceRedDark;
        _navigationPanel.Resize += (_, _) => ResizeNavigationButtons();
        layout.Controls.Add(_navigationPanel, 0, 2);

        var logoutButton = BuildNavigationButton("Đăng xuất", string.Empty, AppTheme.Gold, AppTheme.PoliceRedDark);
        logoutButton.Margin = new Padding(0, 10, 0, 0);
        logoutButton.TextAlign = ContentAlignment.MiddleCenter;
        logoutButton.Padding = new Padding(0);
        logoutButton.Click += (_, _) =>
        {
            LogoutRequested = true;
            Close();
        };
        layout.Controls.Add(logoutButton, 0, 3);

        sidebar.Controls.Add(layout);
        return sidebar;
    }

    private Control BuildMainArea()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = AppTheme.Page,
            Padding = new Padding(28, 24, 28, 24)
        };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Page
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        _pageTitleLabel.Text = "Tổng quan hệ thống";
        _pageTitleLabel.Dock = DockStyle.Fill;
        _pageTitleLabel.Font = AppTheme.Font(22, FontStyle.Bold);
        _pageTitleLabel.ForeColor = AppTheme.Navy;
        _pageTitleLabel.TextAlign = ContentAlignment.MiddleLeft;
        header.Controls.Add(_pageTitleLabel, 0, 0);

        var userInfo = new Label
        {
            Text = $"{_currentUser.DisplayName}\r\n{_currentUser.Username}",
            Dock = DockStyle.Fill,
            Font = AppTheme.Font(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Ink,
            TextAlign = ContentAlignment.MiddleRight
        };
        header.Controls.Add(userInfo, 1, 0);

        main.Controls.Add(header, 0, 0);

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.BackColor = AppTheme.Page;
        main.Controls.Add(_contentPanel, 0, 1);

        return main;
    }

    private async Task LoadPermissionsAsync()
    {
        _canEditAttendanceToday = await _permissionService.CanEditAttendanceTodayAsync(_currentUser, DateTime.Today);
        BuildNavigation();
        ShowModule(GetModules().First(module => module.Key == "dashboard"));
    }

    private void BuildNavigation()
    {
        _navigationPanel.Controls.Clear();

        foreach (var module in GetModules())
        {
            if (!_permissionService.Can(_currentUser, module.Permission))
            {
                continue;
            }

            var isDutyBlocked = module.RequiresDutyToday && !_canEditAttendanceToday;
            var button = BuildNavigationButton(
                module.Title,
                isDutyBlocked ? "Chưa có lịch trực sáng nay" : module.Description,
                Color.White,
                isDutyBlocked ? NavigationDisabledBack : NavigationNormalBack);
            button.Enabled = !isDutyBlocked;
            button.Tag = module.Key;
            button.Click += (_, _) =>
            {
                SetActiveNavigationButton(button);
                ShowModule(module);
            };
            _navigationPanel.Controls.Add(button);
        }

        ResizeNavigationButtons();
        SetActiveNavigationButton(_navigationPanel.Controls.OfType<Button>().FirstOrDefault(button => button.Tag?.ToString() == "dashboard"));
    }

    private void ResizeNavigationButtons()
    {
        var width = Math.Max(204, _navigationPanel.ClientSize.Width - 10);
        foreach (var button in _navigationPanel.Controls.OfType<Button>())
        {
            button.Width = width;
        }
    }

    private void SetActiveNavigationButton(Button? selectedButton)
    {
        foreach (var button in _navigationPanel.Controls.OfType<Button>())
        {
            if (!button.Enabled)
            {
                button.BackColor = NavigationDisabledBack;
                button.FlatAppearance.BorderColor = Color.FromArgb(90, AppTheme.Gold);
                button.FlatAppearance.BorderSize = 1;
                continue;
            }

            var isActive = ReferenceEquals(button, selectedButton);
            button.BackColor = isActive ? NavigationActiveBack : NavigationNormalBack;
            button.FlatAppearance.BorderColor = isActive ? AppTheme.Gold : Color.FromArgb(90, AppTheme.Gold);
            button.FlatAppearance.BorderSize = isActive ? 2 : 1;
        }
    }

    private void ShowModule(ModuleDefinition module)
    {
        _pageTitleLabel.Text = module.Title;
        _contentPanel.Controls.Clear();

        Control view = module.Key switch
        {
            "dashboard" => new DashboardView(_currentUser, _canEditAttendanceToday),
            "attendance_today" => new AttendanceTodayView(_currentUser),
            "daily_report" => new DailyReportView(),
            "monthly_report" => new MonthlyReportView(),
            "export" => new ExportView(),
            "staff" => new StaffManagementView(_currentUser),
            "users" => new UserManagementView(_currentUser),
            "duty_schedule" => new DutyScheduleView(_currentUser),
            "settings" => new SettingsView(_currentUser),
            "audit" => new AuditLogView(),
            "backup" => new BackupView(_currentUser),
            _ => new DashboardView(_currentUser, _canEditAttendanceToday)
        };

        _contentPanel.Controls.Add(view);
    }

    private static Button BuildNavigationButton(string title, string description, Color foreColor, Color backColor)
    {
        var button = new Button
        {
            AutoSize = false,
            Width = 224,
            Height = 56,
            Margin = new Padding(0, 0, 0, 8),
            Text = string.IsNullOrWhiteSpace(description) ? title : $"{title}\r\n{description}",
            TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = foreColor,
            Font = AppTheme.Font(9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Padding = new Padding(14, 0, 10, 0)
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(90, AppTheme.Gold);
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private static IReadOnlyCollection<ModuleDefinition> GetModules()
    {
        return
        [
            new("dashboard", "Tổng quan", "Theo dõi trạng thái hệ thống", Permission.ViewDashboard),
            new("attendance_today", "Điểm danh hôm nay", "Nhập trạng thái cán bộ trước 10h", Permission.TakeAttendance, true),
            new("daily_report", "Báo cáo ngày", "Xem số lượng theo từng trạng thái", Permission.ViewReports),
            new("monthly_report", "Báo cáo tháng", "Tổng hợp chuyên cần theo cán bộ", Permission.ViewReports),
            new("export", "Xuất Excel", "Xuất báo cáo phục vụ lưu trữ", Permission.ExportReports),
            new("staff", "Quản lý cán bộ", "Thêm, sửa, ẩn danh sách cán bộ", Permission.ManageStaff),
            new("users", "Tài khoản và vai trò", "Quản lý người dùng hệ thống", Permission.ManageUsers),
            new("duty_schedule", "Lịch trực ban", "Phân công cán bộ trực theo ngày", Permission.ManageDutySchedule),
            new("settings", "Cấu hình hệ thống", "Thiết lập giờ khóa và thư mục dữ liệu", Permission.ManageSettings),
            new("audit", "Nhật ký thao tác", "Theo dõi lịch sử đăng nhập và chỉnh sửa", Permission.ViewAuditLogs),
            new("backup", "Sao lưu / khôi phục", "Bảo vệ dữ liệu nội bộ", Permission.BackupRestore)
        ];
    }

    private sealed record ModuleDefinition(
        string Key,
        string Title,
        string Description,
        Permission Permission,
        bool RequiresDutyToday = false);
}
