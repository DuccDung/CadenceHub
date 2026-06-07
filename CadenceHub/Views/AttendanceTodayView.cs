using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class AttendanceTodayView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _user;
    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly Label _policyLabel = new();
    private readonly Button _saveButton = ViewHelpers.CommandButton("Lưu điểm danh hôm nay");
    private List<AttendanceEntryRow> _rows = new();
    private List<AttendanceStatusOption> _statuses = new();
    private int _statusColumnIndex = -1;

    public AttendanceTodayView(AuthenticatedUser user)
    {
        _user = user;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = AppTheme.Page
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = AppTheme.Page };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _datePicker.Format = DateTimePickerFormat.Custom;
        _datePicker.CustomFormat = "dd/MM/yyyy";
        _datePicker.Dock = DockStyle.Fill;
        _datePicker.ValueChanged += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(_datePicker, 0, 0);

        var reloadButton = ViewHelpers.CommandButton("Tải lại", AppTheme.DeepGreen);
        reloadButton.Click += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(reloadButton, 1, 0);

        _saveButton.Click += async (_, _) => await SaveAsync();
        toolbar.Controls.Add(_saveButton, 2, 0);

        _policyLabel.Dock = DockStyle.Fill;
        _policyLabel.Font = AppTheme.Font(10.5f, FontStyle.Bold);
        _policyLabel.ForeColor = AppTheme.MutedText;
        _policyLabel.TextAlign = ContentAlignment.MiddleLeft;
        toolbar.Controls.Add(_policyLabel, 3, 0);
        root.Controls.Add(toolbar, 0, 0);

        var card = ViewHelpers.Card();
        ViewHelpers.StyleGrid(_grid);
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AttendanceEntryRow.StaffCode), HeaderText = "Mã", FillWeight = 72, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AttendanceEntryRow.FullName), HeaderText = "Họ tên", FillWeight = 180, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AttendanceEntryRow.Unit), HeaderText = "Đơn vị", FillWeight = 70, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AttendanceEntryRow.PositionName), HeaderText = "Chức vụ", FillWeight = 110, ReadOnly = true });
        _statusColumnIndex = _grid.Columns.Add(new DataGridViewComboBoxColumn { DataPropertyName = nameof(AttendanceEntryRow.StatusId), HeaderText = "Trạng thái", FillWeight = 150, DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AttendanceEntryRow.Note), HeaderText = "Ghi chú", FillWeight = 150 });
        _grid.EditMode = DataGridViewEditMode.EditOnEnter;
        _grid.CellClick += OpenStatusDropDown;
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
            {
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        card.Controls.Add(_grid);
        root.Controls.Add(card, 0, 1);

        Controls.Add(root);
    }

    private void OpenStatusDropDown(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != _statusColumnIndex || _grid.ReadOnly)
        {
            return;
        }

        _grid.CurrentCell = _grid[e.ColumnIndex, e.RowIndex];
        _grid.BeginEdit(true);

        if (_grid.EditingControl is ComboBox comboBox)
        {
            comboBox.DroppedDown = true;
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var date = ViewHelpers.DateOnlyFrom(_datePicker);
            _statuses = await _dataService.GetAttendanceStatusesAsync();
            _rows = await _dataService.GetAttendanceEntriesAsync(date);
            var policy = await _dataService.GetAttendanceEditPolicyAsync(_user, date);

            var statusColumn = (DataGridViewComboBoxColumn)_grid.Columns[4];
            statusColumn.DataSource = _statuses;
            statusColumn.ValueMember = nameof(AttendanceStatusOption.Id);
            statusColumn.DisplayMember = nameof(AttendanceStatusOption.Name);

            _grid.DataSource = null;
            _grid.DataSource = _rows;
            _grid.ReadOnly = !policy.CanEdit;
            foreach (DataGridViewColumn column in _grid.Columns)
            {
                if (column.Index < 4)
                {
                    column.ReadOnly = true;
                }
            }

            _saveButton.Enabled = policy.CanEdit;
            _policyLabel.Text = policy.Message;
            _policyLabel.ForeColor = policy.CanEdit ? AppTheme.Success : AppTheme.Warning;
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

            await _dataService.SaveAttendanceAsync(ViewHelpers.DateOnlyFrom(_datePicker), _rows, _user);
            ViewHelpers.ShowInfo(this, "Đã lưu điểm danh hôm nay.");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
