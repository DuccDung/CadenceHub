using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

public sealed class UserManagementView : UserControl
{
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _actor;
    private readonly DataGridView _grid = new();
    private readonly TextBox _usernameBox = new();
    private readonly TextBox _displayNameBox = new();
    private readonly TextBox _passwordBox = new();
    private readonly ComboBox _staffBox = new();
    private readonly CheckedListBox _rolesBox = new();
    private readonly CheckBox _activeCheck = new() { Text = "Đang hoạt động", Checked = true, Dock = DockStyle.Fill };
    private int _selectedId;
    private List<RoleOption> _roles = new();
    private List<StaffOption> _staff = new();

    public UserManagementView(AuthenticatedUser actor)
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var formCard = ViewHelpers.Card();
        formCard.Margin = new Padding(0, 0, 0, 16);
        var form = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 4, BackColor = AppTheme.Surface };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        AddTextField(form, "Tài khoản", _usernameBox, 0, 0);
        AddTextField(form, "Tên hiển thị", _displayNameBox, 2, 0);
        AddTextField(form, "Mật khẩu mới", _passwordBox, 0, 1);
        _passwordBox.UseSystemPasswordChar = true;
        form.Controls.Add(new Label { Text = "Cán bộ liên kết", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.Font(10, FontStyle.Bold) }, 2, 1);
        _staffBox.Dock = DockStyle.Fill;
        _staffBox.DropDownStyle = ComboBoxStyle.DropDownList;
        form.Controls.Add(_staffBox, 3, 1);

        form.Controls.Add(new Label { Text = "Vai trò", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, Font = AppTheme.Font(10, FontStyle.Bold) }, 0, 2);
        _rolesBox.Dock = DockStyle.Fill;
        form.SetColumnSpan(_rolesBox, 3);
        form.Controls.Add(_rolesBox, 1, 2);
        form.Controls.Add(_activeCheck, 3, 3);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = AppTheme.Surface, Padding = new Padding(0, 8, 0, 0) };
        var newButton = ViewHelpers.CommandButton("Tạo mới", AppTheme.DeepGreen);
        newButton.Click += (_, _) => ClearForm();
        var saveButton = ViewHelpers.CommandButton("Lưu tài khoản");
        saveButton.Click += async (_, _) => await SaveAsync();
        var reloadButton = ViewHelpers.CommandButton("Tải lại", AppTheme.DeepGreen);
        reloadButton.Click += async (_, _) => await LoadDataAsync();
        buttons.Controls.AddRange([newButton, saveButton, reloadButton]);
        form.SetColumnSpan(buttons, 4);
        form.Controls.Add(buttons, 0, 4);

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

    private static void AddTextField(TableLayoutPanel form, string label, TextBox box, int column, int row)
    {
        form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.Font(10, FontStyle.Bold) }, column, row);
        box.Dock = DockStyle.Fill;
        form.Controls.Add(box, column + 1, row);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _roles = await _dataService.GetRoleOptionsAsync();
            _staff = [new StaffOption(0, "(Không liên kết)")];
            _staff.AddRange(await _dataService.GetStaffOptionsAsync(false));

            _staffBox.DataSource = null;
            _staffBox.DataSource = _staff;
            _staffBox.DisplayMember = nameof(StaffOption.Name);
            _staffBox.ValueMember = nameof(StaffOption.Id);

            _rolesBox.Items.Clear();
            foreach (var role in _roles)
            {
                _rolesBox.Items.Add(role);
            }
            _rolesBox.DisplayMember = nameof(RoleOption.Name);

            _grid.DataSource = await _dataService.GetUserRowsAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }

    private void FillSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not UserEditorRow row)
        {
            return;
        }

        _selectedId = row.Id;
        _usernameBox.Text = row.Username;
        _displayNameBox.Text = row.DisplayName;
        _passwordBox.Clear();
        _activeCheck.Checked = row.IsActive;
        _staffBox.SelectedValue = row.StaffId ?? 0;

        var assignedCodes = row.Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < _rolesBox.Items.Count; i++)
        {
            var role = (RoleOption)_rolesBox.Items[i];
            _rolesBox.SetItemChecked(i, assignedCodes.Contains(role.Code));
        }
    }

    private void ClearForm()
    {
        _selectedId = 0;
        _usernameBox.Clear();
        _displayNameBox.Clear();
        _passwordBox.Clear();
        _activeCheck.Checked = true;
        _staffBox.SelectedValue = 0;
        for (var i = 0; i < _rolesBox.Items.Count; i++)
        {
            _rolesBox.SetItemChecked(i, false);
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            var selectedRoles = _rolesBox.CheckedItems.Cast<RoleOption>().Select(role => role.Id).ToArray();
            if (string.IsNullOrWhiteSpace(_usernameBox.Text) || string.IsNullOrWhiteSpace(_displayNameBox.Text) || selectedRoles.Length == 0)
            {
                MessageBox.Show(this, "Tài khoản, tên hiển thị và ít nhất một vai trò là bắt buộc.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await _dataService.SaveUserAsync(new UserEditorInput
            {
                Id = _selectedId,
                Username = _usernameBox.Text,
                DisplayName = _displayNameBox.Text,
                NewPassword = _passwordBox.Text,
                StaffId = _staffBox.SelectedValue is int staffId && staffId > 0 ? staffId : null,
                IsActive = _activeCheck.Checked,
                RoleIds = selectedRoles
            }, _actor);

            ViewHelpers.ShowInfo(this, "Đã lưu tài khoản.");
            ClearForm();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ViewHelpers.ShowError(this, ex);
        }
    }
}
