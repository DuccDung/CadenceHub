using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class DailyReportView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly ExcelExportService _exportService = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly DataGridView _summaryGrid = new();
    private readonly DataGridView _detailGrid = new();

    public DailyReportView()
    {
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = AppTheme.Page };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 230));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.Page, WrapContents = false };
        _datePicker.Format = DateTimePickerFormat.Custom;
        _datePicker.CustomFormat = "dd/MM/yyyy";
        _datePicker.Width = 180;
        toolbar.Controls.Add(_datePicker);
        var loadButton = ViewHelpers.CommandButton("Xem báo cáo", AppTheme.DeepGreen);
        loadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(loadButton);
        var exportButton = ViewHelpers.CommandButton("Xuất Excel");
        exportButton.Click += async (_, _) => await ExportAsync();
        toolbar.Controls.Add(exportButton);
        root.Controls.Add(toolbar, 0, 0);

        root.Controls.Add(BuildGridCard("Tổng hợp theo trạng thái", _summaryGrid), 0, 1);
        root.Controls.Add(BuildGridCard("Chi tiết điểm danh trong ngày", _detailGrid), 0, 2);
        Controls.Add(root);
    }

    private static Control BuildGridCard(string title, DataGridView grid)
    {
        var card = ViewHelpers.Card();
        card.Margin = new Padding(0, 0, 0, 16);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = AppTheme.Surface };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(ViewHelpers.Title(title), 0, 0);
        ViewHelpers.StyleGrid(grid);
        grid.ReadOnly = true;
        layout.Controls.Add(grid, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var date = ViewHelpers.DateOnlyFrom(_datePicker);
            _summaryGrid.DataSource = await _dataService.GetDailySummaryAsync(date);
            _detailGrid.DataSource = await _dataService.GetDailyDetailsAsync(date);
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private async Task ExportAsync()
    {
        try
        {
            var path = await _exportService.ExportDailyReportAsync(ViewHelpers.DateOnlyFrom(_datePicker));
            ViewHelpers.ShowInfo(this, $"Đã xuất báo cáo:\r\n{path}");
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
