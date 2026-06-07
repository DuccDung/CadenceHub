using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class DashboardView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _user;
    private readonly bool _canEditAttendanceToday;
    private readonly TableLayoutPanel _cards = new();
    private readonly DataGridView _summaryGrid = new();
    private readonly Label _messageLabel = new();

    public DashboardView(AuthenticatedUser user, bool canEditAttendanceToday)
    {
        _user = user;
        _canEditAttendanceToday = canEditAttendanceToday;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = AppTheme.Page
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var welcome = ViewHelpers.Card(new Padding(22));
        welcome.Margin = new Padding(0, 0, 0, 16);
        var welcomeLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = AppTheme.Surface };
        welcomeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        welcomeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        welcomeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        welcomeLayout.Controls.Add(ViewHelpers.Title("Tổng quan vận hành hôm nay"), 0, 0);
        welcomeLayout.Controls.Add(ViewHelpers.InfoLabel($"Người dùng: {_user.DisplayName} | Vai trò: {string.Join(", ", _user.RoleNames)}"), 0, 1);
        _messageLabel.Text = _canEditAttendanceToday ? "Tài khoản hiện có quyền thao tác điểm danh hôm nay." : "Tài khoản không có lịch trực hoặc không đủ quyền điểm danh hôm nay.";
        _messageLabel.ForeColor = _canEditAttendanceToday ? AppTheme.Success : AppTheme.Warning;
        _messageLabel.Dock = DockStyle.Fill;
        _messageLabel.Font = AppTheme.Font(10.5f, FontStyle.Bold);
        welcomeLayout.Controls.Add(_messageLabel, 0, 2);
        welcome.Controls.Add(welcomeLayout);
        root.Controls.Add(welcome, 0, 0);

        _cards.Dock = DockStyle.Fill;
        _cards.ColumnCount = 5;
        _cards.RowCount = 1;
        _cards.BackColor = AppTheme.Page;
        for (var i = 0; i < 5; i++)
        {
            _cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        }
        root.Controls.Add(_cards, 0, 1);

        var summaryCard = ViewHelpers.Card();
        summaryCard.Margin = new Padding(0, 16, 0, 0);
        var summaryLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = AppTheme.Surface };
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        summaryLayout.Controls.Add(ViewHelpers.Title("Tổng hợp trạng thái điểm danh"), 0, 0);
        ViewHelpers.StyleGrid(_summaryGrid);
        _summaryGrid.ReadOnly = true;
        summaryLayout.Controls.Add(_summaryGrid, 0, 1);
        summaryCard.Controls.Add(summaryLayout);
        root.Controls.Add(summaryCard, 0, 2);

        Controls.Add(root);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var snapshot = await _dataService.GetDashboardAsync(DateOnly.FromDateTime(DateTime.Today));
            _cards.Controls.Clear();
            _cards.Controls.Add(BuildMetricCard("Tổng cán bộ", snapshot.TotalStaff.ToString(), AppTheme.Navy), 0, 0);
            _cards.Controls.Add(BuildMetricCard("Đã điểm danh", snapshot.RecordedCount.ToString(), AppTheme.Success), 1, 0);
            _cards.Controls.Add(BuildMetricCard("Chưa ghi nhận", snapshot.NotRecordedCount.ToString(), AppTheme.Warning), 2, 0);
            _cards.Controls.Add(BuildMetricCard("Có mặt/nhiệm vụ", snapshot.PresentCount.ToString(), AppTheme.DeepGreen), 3, 0);
            _cards.Controls.Add(BuildMetricCard("Vắng/cần theo dõi", snapshot.AbsentCount.ToString(), AppTheme.Danger), 4, 0);
            _summaryGrid.DataSource = snapshot.StatusSummary.ToList();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private static Control BuildMetricCard(string title, string value, Color accent)
    {
        var card = ViewHelpers.Card(new Padding(18));
        card.Margin = new Padding(0, 0, 14, 0);
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 44,
            Font = AppTheme.Font(10f, FontStyle.Bold),
            ForeColor = AppTheme.MutedText,
            AutoEllipsis = false,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var valueLabel = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            Font = AppTheme.Font(22, FontStyle.Bold),
            ForeColor = accent,
            TextAlign = ContentAlignment.MiddleLeft
        };
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        return card;
    }
}
