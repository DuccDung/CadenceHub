using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class StaffManagementView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _user;
    private readonly DataGridView _grid = new();
    private readonly TextBox _codeBox = new();
    private readonly TextBox _nameBox = new();
    private readonly TextBox _unitBox = new();
    private readonly TextBox _positionCodeBox = new();
    private readonly TextBox _positionNameBox = new();
    private readonly CheckBox _activeCheck = new() { Text = "Đang hoạt động", Checked = true, Dock = DockStyle.Fill };
    private int _selectedId;

    public StaffManagementView(AuthenticatedUser user)
    {
        _user = user;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = AppTheme.Page };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 245));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var formCard = ViewHelpers.Card();
        formCard.Margin = new Padding(0, 0, 0, 16);
        var form = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 4, BackColor = AppTheme.Surface };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        AddField(form, "Mã cán bộ", _codeBox, 0);
        AddField(form, "Họ tên", _nameBox, 1);
        AddField(form, "Đơn vị", _unitBox, 2);
        AddField(form, "Mã chức vụ", _positionCodeBox, 3);
        form.Controls.Add(new Label { Text = "Tên chức vụ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.Font(10, FontStyle.Bold) }, 0, 2);
        _positionNameBox.Dock = DockStyle.Fill;
        form.Controls.Add(_positionNameBox, 1, 2);
        form.Controls.Add(_activeCheck, 3, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = AppTheme.Surface, Padding = new Padding(0, 8, 0, 0) };
        var newButton = ViewHelpers.CommandButton("Tạo mới", AppTheme.DeepGreen);
        newButton.Click += (_, _) => ClearForm();
        var saveButton = ViewHelpers.CommandButton("Lưu cán bộ");
        saveButton.Click += async (_, _) => await SaveAsync();
        var importButton = ViewHelpers.CommandButton("Import Excel", AppTheme.Navy);
        importButton.Click += async (_, _) => await ImportAsync();
        var reloadButton = ViewHelpers.CommandButton("Tải lại", AppTheme.DeepGreen);
        reloadButton.Click += async (_, _) => await LoadDataAsync();
        buttons.Controls.AddRange([newButton, saveButton, importButton, reloadButton]);
        form.SetColumnSpan(buttons, 4);
        form.Controls.Add(buttons, 0, 3);

        formCard.Controls.Add(form);
        root.Controls.Add(formCard, 0, 0);

        var gridCard = ViewHelpers.Card();
        ViewHelpers.StyleGrid(_grid);
        _grid.ReadOnly = true;
        _grid.SelectionChanged += (_, _) => FillSelected();
        gridCard.Controls.Add(_grid);
        root.Controls.Add(gridCard, 0, 1);
        Controls.Add(root);
    }

    private static void AddField(TableLayoutPanel form, string label, TextBox box, int row)
    {
        var labelColumn = row % 2 == 0 ? 0 : 2;
        var inputColumn = row % 2 == 0 ? 1 : 3;
        var targetRow = row / 2;
        form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.Font(10, FontStyle.Bold) }, labelColumn, targetRow);
        box.Dock = DockStyle.Fill;
        form.Controls.Add(box, inputColumn, targetRow);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _grid.DataSource = await _dataService.GetStaffRowsAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private void FillSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not StaffEditorRow row)
        {
            return;
        }

        _selectedId = row.Id;
        _codeBox.Text = row.StaffCode;
        _nameBox.Text = row.FullName;
        _unitBox.Text = row.Unit;
        _positionCodeBox.Text = row.PositionCode;
        _positionNameBox.Text = row.PositionName;
        _activeCheck.Checked = row.IsActive;
    }

    private void ClearForm()
    {
        _selectedId = 0;
        _codeBox.Clear();
        _nameBox.Clear();
        _unitBox.Text = "NVĐT";
        _positionCodeBox.Clear();
        _positionNameBox.Clear();
        _activeCheck.Checked = true;
    }

    private async Task SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_codeBox.Text) || string.IsNullOrWhiteSpace(_nameBox.Text) || string.IsNullOrWhiteSpace(_unitBox.Text))
            {
                MessageBox.Show(this, "Mã cán bộ, họ tên và đơn vị là bắt buộc.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await _dataService.SaveStaffAsync(new StaffEditorRow
            {
                Id = _selectedId,
                StaffCode = _codeBox.Text,
                FullName = _nameBox.Text,
                Unit = _unitBox.Text,
                PositionCode = _positionCodeBox.Text,
                PositionName = _positionNameBox.Text,
                IsActive = _activeCheck.Checked
            }, _user);

            ViewHelpers.ShowInfo(this, "Đã lưu cán bộ.");
            ClearForm();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private async Task ImportAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Chọn file danh sách cán bộ",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _dataService.ImportStaffFromExcelAsync(dialog.FileName, _user);
            ViewHelpers.ShowInfo(this, "Đã import danh sách cán bộ.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
