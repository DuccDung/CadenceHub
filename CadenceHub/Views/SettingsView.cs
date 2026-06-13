using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class SettingsView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _actor;
    private readonly DataGridView _grid = new();
    private readonly Button _saveButton = ViewHelpers.CommandButton("Lưu cấu hình");
    private readonly Button _reloadButton = ViewHelpers.CommandButton("Tải lại", AppTheme.DeepGreen);
    private bool _isBusy;

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
        _saveButton.Click += async (_, _) => await SaveAsync();
        _reloadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.AddRange([_saveButton, _reloadButton]);
        root.Controls.Add(toolbar, 0, 0);

        var card = ViewHelpers.Card();
        ViewHelpers.StyleGrid(_grid);
        _grid.AutoGenerateColumns = true;
        _grid.SelectionChanged += (_, _) => UpdateButtonStates();
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

            _grid.DataSource = null;
            _grid.DataSource = await _dataService.GetSettingsAsync();
            if (_grid.Columns[nameof(SettingRow.Key)] is { } keyColumn) keyColumn.ReadOnly = true;
            if (_grid.Columns[nameof(SettingRow.Description)] is { } descColumn) descColumn.ReadOnly = true;
            if (_grid.Columns[nameof(SettingRow.UpdatedAt)] is { } updatedColumn) updatedColumn.ReadOnly = true;
            ViewHelpers.ClearGridSelection(_grid);
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

    private async Task SaveAsync()
    {
        try
        {
            if (!_saveButton.Enabled)
            {
                return;
            }

            _isBusy = true;
            UpdateButtonStates();
            _grid.EndEdit();
            if (_grid.CurrentRow?.DataBoundItem is not SettingRow row)
            {
                return;
            }

            await _dataService.SaveSettingAsync(row, _actor);
            ViewHelpers.ShowInfo(this, "Đã lưu cấu hình đang chọn.");
            _isBusy = false;
            await LoadDataAsync();
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
        ViewHelpers.SetButtonState(_saveButton, !_isBusy && _grid.CurrentRow?.DataBoundItem is SettingRow, AppTheme.PoliceRed);
        ViewHelpers.SetButtonState(_reloadButton, !_isBusy, AppTheme.DeepGreen);
    }
}
