using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class MonthlyReportView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly ExcelExportService _exportService = new();
    private readonly DateTimePicker _monthPicker = new();
    private readonly DataGridView _grid = new();

    public MonthlyReportView()
    {
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = AppTheme.Page };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.Page, WrapContents = false };
        _monthPicker.Format = DateTimePickerFormat.Custom;
        _monthPicker.CustomFormat = "yyyy-MM";
        _monthPicker.Width = 180;
        toolbar.Controls.Add(_monthPicker);
        var loadButton = ViewHelpers.CommandButton("Tạo báo cáo", AppTheme.DeepGreen);
        loadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(loadButton);
        var exportButton = ViewHelpers.CommandButton("Xuất Excel");
        exportButton.Click += async (_, _) => await ExportAsync();
        toolbar.Controls.Add(exportButton);
        root.Controls.Add(toolbar, 0, 0);

        var card = ViewHelpers.Card();
        ViewHelpers.StyleGrid(_grid);
        _grid.ReadOnly = true;
        card.Controls.Add(_grid);
        root.Controls.Add(card, 0, 1);
        Controls.Add(root);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _grid.DataSource = await _dataService.GetMonthlySummaryAsync(_monthPicker.Value.Year, _monthPicker.Value.Month);
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
            var path = await _exportService.ExportMonthlyReportAsync(_monthPicker.Value.Year, _monthPicker.Value.Month);
            ViewHelpers.ShowInfo(this, $"Đã xuất báo cáo:\r\n{path}");
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
