using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class ExportView : UserControl
{
    private readonly ExcelExportService _exportService = new();
    private readonly ComboBox _reportTypeBox = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly Label _outputLabel = new();

    public ExportView()
    {
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
    }

    private void BuildLayout()
    {
        var card = ViewHelpers.Card(new Padding(28));
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 230, RowCount = 5, ColumnCount = 2, BackColor = AppTheme.Surface };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 5; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        }

        layout.Controls.Add(ViewHelpers.Title("Loại báo cáo"), 0, 0);
        _reportTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _reportTypeBox.Items.AddRange(["Báo cáo ngày", "Báo cáo tháng"]);
        _reportTypeBox.SelectedIndex = 0;
        _reportTypeBox.Dock = DockStyle.Fill;
        layout.Controls.Add(_reportTypeBox, 1, 0);

        layout.Controls.Add(ViewHelpers.Title("Ngày/tháng"), 0, 1);
        _datePicker.Format = DateTimePickerFormat.Custom;
        _datePicker.CustomFormat = "dd/MM/yyyy";
        _datePicker.Dock = DockStyle.Left;
        _datePicker.Width = 180;
        _reportTypeBox.SelectedIndexChanged += (_, _) =>
        {
            _datePicker.CustomFormat = _reportTypeBox.SelectedIndex == 0 ? "dd/MM/yyyy" : "yyyy-MM";
        };
        layout.Controls.Add(_datePicker, 1, 1);

        var exportButton = ViewHelpers.CommandButton("Xuất Excel");
        exportButton.Click += async (_, _) => await ExportAsync();
        layout.Controls.Add(exportButton, 1, 2);

        _outputLabel.Dock = DockStyle.Fill;
        _outputLabel.Font = AppTheme.Font(10.5f, FontStyle.Bold);
        _outputLabel.ForeColor = AppTheme.DeepGreen;
        layout.Controls.Add(_outputLabel, 1, 3);

        card.Controls.Add(layout);
        Controls.Add(card);
    }

    private async Task ExportAsync()
    {
        try
        {
            var path = _reportTypeBox.SelectedIndex == 0
                ? await _exportService.ExportDailyReportAsync(DateOnly.FromDateTime(_datePicker.Value.Date))
                : await _exportService.ExportMonthlyReportAsync(_datePicker.Value.Year, _datePicker.Value.Month);
            _outputLabel.Text = path;
            ViewHelpers.ShowInfo(this, $"Đã xuất file Excel:\r\n{path}");
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
