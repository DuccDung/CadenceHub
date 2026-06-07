using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class DutyScheduleView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _actor;
    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _fromPicker = new();
    private readonly DateTimePicker _toPicker = new();
    private readonly DateTimePicker _dutyDatePicker = new();
    private readonly ComboBox _shiftBox = new();
    private readonly TextBox _staffTextBox = new();
    private readonly Button _staffSearchButton = new();
    private readonly TextBox _noteBox = new();
    private int _selectedId;
    private int _selectedStaffId;
    private List<LinkedStaffOption> _staffOptions = new();
    private bool _isOpeningStaffSearch;

    public DutyScheduleView(AuthenticatedUser actor)
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 225));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var formCard = ViewHelpers.Card();
        formCard.Margin = new Padding(0, 0, 0, 16);
        var form = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 4, BackColor = AppTheme.Surface };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        _fromPicker.Format = DateTimePickerFormat.Custom;
        _fromPicker.CustomFormat = "dd/MM/yyyy";
        _toPicker.Format = DateTimePickerFormat.Custom;
        _toPicker.CustomFormat = "dd/MM/yyyy";
        _toPicker.Value = DateTime.Today.AddDays(30);
        _dutyDatePicker.Format = DateTimePickerFormat.Custom;
        _dutyDatePicker.CustomFormat = "dd/MM/yyyy";

        AddControlField(form, "Từ ngày", _fromPicker, 0, 0);
        AddControlField(form, "Đến ngày", _toPicker, 2, 0);
        AddControlField(form, "Ngày trực", _dutyDatePicker, 0, 1);
        _shiftBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _shiftBox.Items.AddRange(["FULL_DAY", "MORNING", "AFTERNOON"]);
        _shiftBox.SelectedIndex = 0;
        AddControlField(form, "Ca trực", _shiftBox, 2, 1);
        AddControlField(form, "Cán bộ", BuildStaffPicker(), 0, 2);
        AddControlField(form, "Ghi chú", _noteBox, 2, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = AppTheme.Surface, Padding = new Padding(0, 8, 0, 0) };
        var newButton = ViewHelpers.CommandButton("Tạo mới", AppTheme.DeepGreen);
        newButton.Click += (_, _) => ClearForm();
        var saveButton = ViewHelpers.CommandButton("Lưu lịch trực");
        saveButton.Click += async (_, _) => await SaveAsync();
        var deleteButton = ViewHelpers.CommandButton("Xóa", AppTheme.Danger);
        deleteButton.Click += async (_, _) => await DeleteAsync();
        var loadButton = ViewHelpers.CommandButton("Tải danh sách", AppTheme.DeepGreen);
        loadButton.Click += async (_, _) => await LoadDataAsync();
        buttons.Controls.AddRange([newButton, saveButton, deleteButton, loadButton]);
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

    private Control BuildStaffPicker()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = AppTheme.Surface, Margin = Padding.Empty };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Cursor = Cursors.Hand;
        panel.Click += async (_, _) => await OpenStaffSearchAsync();

        _staffTextBox.Dock = DockStyle.Fill;
        _staffTextBox.ReadOnly = true;
        _staffTextBox.BackColor = Color.White;
        _staffTextBox.PlaceholderText = "Chọn cán bộ trực";
        _staffTextBox.Cursor = Cursors.Hand;
        _staffTextBox.Click += async (_, _) => await OpenStaffSearchAsync();
        _staffTextBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode is Keys.Enter or Keys.F4)
            {
                await OpenStaffSearchAsync();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        };

        _staffSearchButton.Text = "Chọn";
        _staffSearchButton.Dock = DockStyle.Fill;
        _staffSearchButton.FlatStyle = FlatStyle.Flat;
        _staffSearchButton.BackColor = AppTheme.Navy;
        _staffSearchButton.ForeColor = Color.White;
        _staffSearchButton.Font = AppTheme.Font(10, FontStyle.Bold);
        _staffSearchButton.Cursor = Cursors.Hand;
        _staffSearchButton.Margin = new Padding(6, 0, 0, 0);
        _staffSearchButton.FlatAppearance.BorderSize = 0;
        _staffSearchButton.Click += async (_, _) => await OpenStaffSearchAsync();

        panel.Controls.Add(_staffTextBox, 0, 0);
        panel.Controls.Add(_staffSearchButton, 1, 0);
        return panel;
    }

    private static void AddControlField(TableLayoutPanel form, string label, Control control, int column, int row)
    {
        form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.Font(10, FontStyle.Bold) }, column, row);
        control.Dock = DockStyle.Fill;
        form.Controls.Add(control, column + 1, row);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _staffOptions = await _dataService.GetLinkedStaffOptionsAsync();
            RefreshSelectedStaffText();
            _grid.DataSource = await _dataService.GetDutySchedulesAsync(ViewHelpers.DateOnlyFrom(_fromPicker), ViewHelpers.DateOnlyFrom(_toPicker));
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private void FillSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DutyScheduleRow row)
        {
            return;
        }

        _selectedId = row.Id;
        _dutyDatePicker.Value = row.DutyDate.ToDateTime(TimeOnly.MinValue);
        _shiftBox.SelectedItem = row.ShiftCode;
        SetSelectedStaff(row.StaffId, row.StaffName);
        _noteBox.Text = row.Note;
    }

    private void ClearForm()
    {
        _selectedId = 0;
        _selectedStaffId = 0;
        _staffTextBox.Clear();
        _dutyDatePicker.Value = DateTime.Today;
        _shiftBox.SelectedIndex = 0;
        _noteBox.Clear();
    }

    private async Task OpenStaffSearchAsync()
    {
        if (_isOpeningStaffSearch)
        {
            return;
        }

        _isOpeningStaffSearch = true;

        try
        {
            _staffOptions = await _dataService.GetLinkedStaffOptionsAsync();
            RefreshSelectedStaffText();

            if (_staffOptions.Count == 0)
            {
                MessageBox.Show(this, "Chưa có cán bộ đang hoạt động nào được liên kết tài khoản đang hoạt động.", "Không có dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new StaffSearchDialog(_staffOptions, _selectedStaffId);
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedStaff is not null)
            {
                SetSelectedStaff(dialog.SelectedStaff);
            }
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
        finally
        {
            _isOpeningStaffSearch = false;
        }
    }

    private void SetSelectedStaff(LinkedStaffOption staff)
    {
        _selectedStaffId = staff.Id;
        _staffTextBox.Text = staff.DisplayName;
    }

    private void SetSelectedStaff(int staffId, string staffName)
    {
        _selectedStaffId = staffId;
        RefreshSelectedStaffText(staffName);
    }

    private void RefreshSelectedStaffText(string fallbackName = "")
    {
        if (_selectedStaffId <= 0)
        {
            _staffTextBox.Clear();
            return;
        }

        var staff = _staffOptions.FirstOrDefault(item => item.Id == _selectedStaffId);
        _staffTextBox.Text = staff?.DisplayName ?? fallbackName;
    }

    private async Task SaveAsync()
    {
        try
        {
            if (_selectedStaffId <= 0)
            {
                MessageBox.Show(this, "Vui lòng chọn cán bộ trực.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await _dataService.SaveDutyScheduleAsync(new DutyScheduleRow
            {
                Id = _selectedId,
                DutyDate = ViewHelpers.DateOnlyFrom(_dutyDatePicker),
                ShiftCode = _shiftBox.SelectedItem?.ToString() ?? "FULL_DAY",
                StaffId = _selectedStaffId,
                Note = _noteBox.Text
            }, _actor);

            ViewHelpers.ShowInfo(this, "Đã lưu lịch trực.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private async Task DeleteAsync()
    {
        if (_selectedId == 0)
        {
            return;
        }

        if (MessageBox.Show(this, "Xóa lịch trực đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _dataService.DeleteDutyScheduleAsync(_selectedId, _actor);
            ClearForm();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
