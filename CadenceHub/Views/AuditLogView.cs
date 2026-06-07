using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class AuditLogView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly DateTimePicker _fromPicker = new();
    private readonly DateTimePicker _toPicker = new();
    private readonly DataGridView _grid = new();

    public AuditLogView()
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

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = AppTheme.Page };
        _fromPicker.Format = DateTimePickerFormat.Custom;
        _fromPicker.CustomFormat = "dd/MM/yyyy";
        _fromPicker.Value = DateTime.Today.AddDays(-7);
        _toPicker.Format = DateTimePickerFormat.Custom;
        _toPicker.CustomFormat = "dd/MM/yyyy";
        toolbar.Controls.Add(_fromPicker);
        toolbar.Controls.Add(_toPicker);
        var loadButton = ViewHelpers.CommandButton("Xem nhật ký", AppTheme.DeepGreen);
        loadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(loadButton);
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
            _grid.DataSource = await _dataService.GetAuditLogsAsync(ViewHelpers.DateOnlyFrom(_fromPicker), ViewHelpers.DateOnlyFrom(_toPicker));
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
