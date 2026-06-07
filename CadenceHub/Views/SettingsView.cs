using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class SettingsView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _actor;
    private readonly DataGridView _grid = new();

    public SettingsView(AuthenticatedUser actor)
    {
        _actor = actor;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = AppTheme.Page };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = AppTheme.Page };
        var saveButton = ViewHelpers.CommandButton("Lưu cấu hình");
        saveButton.Click += async (_, _) => await SaveAsync();
        var reloadButton = ViewHelpers.CommandButton("Tải lại", AppTheme.DeepGreen);
        reloadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.AddRange([saveButton, reloadButton]);
        root.Controls.Add(toolbar, 0, 0);

        var card = ViewHelpers.Card();
        ViewHelpers.StyleGrid(_grid);
        _grid.AutoGenerateColumns = true;
        card.Controls.Add(_grid);
        root.Controls.Add(card, 0, 1);
        Controls.Add(root);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _grid.DataSource = await _dataService.GetSettingsAsync();
            if (_grid.Columns[nameof(SettingRow.Key)] is { } keyColumn) keyColumn.ReadOnly = true;
            if (_grid.Columns[nameof(SettingRow.Description)] is { } descColumn) descColumn.ReadOnly = true;
            if (_grid.Columns[nameof(SettingRow.UpdatedAt)] is { } updatedColumn) updatedColumn.ReadOnly = true;
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _grid.EndEdit();
            if (_grid.CurrentRow?.DataBoundItem is not SettingRow row)
            {
                return;
            }

            await _dataService.SaveSettingAsync(row, _actor);
            ViewHelpers.ShowInfo(this, "Đã lưu cấu hình đang chọn.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
