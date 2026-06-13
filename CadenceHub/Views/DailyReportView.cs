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
    private readonly Button _loadButton = ViewHelpers.CommandButton("Xem báo cáo", AppTheme.DeepGreen);
    private readonly Button _exportButton = ViewHelpers.CommandButton("Xuất Excel");
    private bool _isBusy;

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
        _loadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(_loadButton);
        _exportButton.Click += async (_, _) => await ExportAsync();
        toolbar.Controls.Add(_exportButton);
        root.Controls.Add(toolbar, 0, 0);

        root.Controls.Add(BuildGridCard("Tổng hợp theo trạng thái", _summaryGrid), 0, 1);
        root.Controls.Add(BuildGridCard("Chi tiết điểm danh trong ngày", _detailGrid), 0, 2);
        Controls.Add(root);
        UpdateButtonStates();
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
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            UpdateButtonStates();

            var date = ViewHelpers.DateOnlyFrom(_datePicker);
            _summaryGrid.DataSource = await _dataService.GetDailySummaryAsync(date);
            _detailGrid.DataSource = await _dataService.GetDailyDetailsAsync(date);
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
        finally
        {
            _isBusy = false;
            UpdateButtonStates();
        }
    }

    private async Task ExportAsync()
    {
        try
        {
            if (_isBusy)
            {
                return;
            }

            var date = ViewHelpers.DateOnlyFrom(_datePicker);
            var outputPath = ViewHelpers.PickExcelSavePath(this, $"bao_cao_ngay_{date:yyyyMMdd}.xlsx");
            if (outputPath is null)
            {
                return;
            }

            _isBusy = true;
            UpdateButtonStates();

            var path = await _exportService.ExportDailyReportAsync(date, outputPath);
            ViewHelpers.ShowInfo(this, $"Đã xuất báo cáo:\r\n{path}");
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
        finally
        {
            _isBusy = false;
            UpdateButtonStates();
        }
    }

    private void UpdateButtonStates()
    {
        ViewHelpers.SetButtonState(_loadButton, !_isBusy, AppTheme.DeepGreen);
        ViewHelpers.SetButtonState(_exportButton, !_isBusy, AppTheme.PoliceRed);
    }
}
