using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class MonthlyReportView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly ExcelExportService _exportService = new();
    private readonly DateTimePicker _monthPicker = new();
    private readonly DataGridView _grid = new();
    private readonly Button _loadButton = ViewHelpers.CommandButton("Tạo báo cáo", AppTheme.DeepGreen);
    private readonly Button _exportButton = ViewHelpers.CommandButton("Xuất Excel");
    private bool _isBusy;

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
        _loadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(_loadButton);
        _exportButton.Click += async (_, _) => await ExportAsync();
        toolbar.Controls.Add(_exportButton);
        root.Controls.Add(toolbar, 0, 0);

        var card = ViewHelpers.Card();
        ViewHelpers.StyleGrid(_grid);
        _grid.ReadOnly = true;
        card.Controls.Add(_grid);
        root.Controls.Add(card, 0, 1);
        Controls.Add(root);
        UpdateButtonStates();
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

            _grid.DataSource = await _dataService.GetMonthlySummaryAsync(_monthPicker.Value.Year, _monthPicker.Value.Month);
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

            var year = _monthPicker.Value.Year;
            var month = _monthPicker.Value.Month;
            var outputPath = ViewHelpers.PickExcelSavePath(this, $"bao_cao_thang_{year:0000}_{month:00}.xlsx");
            if (outputPath is null)
            {
                return;
            }

            _isBusy = true;
            UpdateButtonStates();

            var path = await _exportService.ExportMonthlyReportAsync(year, month, outputPath);
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
