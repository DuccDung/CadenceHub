using System.Globalization;
using System.Text;
using System.ComponentModel;
using CadenceHub.Services;
using CadenceHub.UI;

namespace CadenceHub.Views;

internal sealed class StaffSearchDialog : Form
{
    private readonly IReadOnlyList<LinkedStaffOption> _allStaff;
    private readonly int _initialStaffId;
    private readonly TextBox _searchBox = new();
    private readonly DataGridView _grid = new();
    private readonly Button _selectButton = ViewHelpers.CommandButton("Chọn", AppTheme.DeepGreen);

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LinkedStaffOption? SelectedStaff { get; private set; }

    public StaffSearchDialog(IReadOnlyList<LinkedStaffOption> staff, int selectedStaffId)
    {
        _allStaff = staff;
        _initialStaffId = selectedStaffId;

        Text = "Tìm cán bộ trực";
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        BackColor = AppTheme.Page;
        Font = AppTheme.Font(10);
        ClientSize = new Size(780, 500);
        MinimumSize = new Size(680, 420);

        BuildLayout();
        ApplyFilter();
        Shown += (_, _) => _searchBox.Focus();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(16), BackColor = AppTheme.Page };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        _searchBox.Dock = DockStyle.Fill;
        _searchBox.PlaceholderText = "Tìm theo mã, họ tên, đơn vị hoặc tài khoản";
        _searchBox.TextChanged += (_, _) => ApplyFilter();
        root.Controls.Add(_searchBox, 0, 0);

        ViewHelpers.StyleGrid(_grid);
        _grid.AutoGenerateColumns = false;
        _grid.ReadOnly = true;
        AddGridColumns();
        _grid.SelectionChanged += (_, _) => UpdateButtonStates();
        _grid.CellDoubleClick += (_, _) => ConfirmSelection();
        _grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        };
        root.Controls.Add(_grid, 0, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = AppTheme.Page, Padding = new Padding(0, 12, 0, 0) };
        var cancelButton = ViewHelpers.CommandButton("Đóng", AppTheme.MutedText);
        cancelButton.DialogResult = DialogResult.Cancel;
        _selectButton.Click += (_, _) => ConfirmSelection();
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(_selectButton);
        root.Controls.Add(buttons, 0, 2);

        AcceptButton = _selectButton;
        CancelButton = cancelButton;
        Controls.Add(root);
        UpdateButtonStates();
    }

    private void ApplyFilter()
    {
        var query = NormalizeSearchText(_searchBox.Text);
        var tokens = query.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allStaff.ToList()
            : _allStaff
                .Where(staff =>
                {
                    var searchable = NormalizeSearchText($"{staff.StaffCode} {staff.FullName} {staff.Unit} {staff.Usernames}");
                    return tokens.All(token => searchable.Contains(token, StringComparison.Ordinal));
                })
                .ToList();

        _grid.DataSource = null;
        _grid.DataSource = filtered;
        if (_initialStaffId > 0 && SelectRowById(_initialStaffId))
        {
            return;
        }

        ClearGridSelection();
    }

    private void AddGridColumns()
    {
        _grid.Columns.Add(BuildTextColumn(nameof(LinkedStaffOption.StaffCode), "Mã cán bộ", 110, 90));
        _grid.Columns.Add(BuildTextColumn(nameof(LinkedStaffOption.FullName), "Họ tên", 230, 170));
        _grid.Columns.Add(BuildTextColumn(nameof(LinkedStaffOption.Unit), "Đơn vị", 120, 90));
        _grid.Columns.Add(BuildTextColumn(nameof(LinkedStaffOption.Usernames), "Tài khoản", 170, 120));
    }

    private static DataGridViewTextBoxColumn BuildTextColumn(string propertyName, string header, float fillWeight, int minimumWidth)
    {
        return new DataGridViewTextBoxColumn
        {
            Name = propertyName,
            DataPropertyName = propertyName,
            HeaderText = header,
            FillWeight = fillWeight,
            MinimumWidth = minimumWidth,
            ReadOnly = true
        };
    }

    private bool SelectRowById(int staffId)
    {
        if (staffId <= 0)
        {
            return false;
        }

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is not LinkedStaffOption staff || staff.Id != staffId)
            {
                continue;
            }

            row.Selected = true;
            _grid.CurrentCell = row.Cells[nameof(LinkedStaffOption.FullName)];
            return true;
        }

        return false;
    }

    private void ClearGridSelection()
    {
        ViewHelpers.ClearGridSelection(_grid);
        UpdateButtonStates();
    }

    private void ConfirmSelection()
    {
        if (!_selectButton.Enabled)
        {
            return;
        }

        if (_grid.CurrentRow?.DataBoundItem is not LinkedStaffOption staff)
        {
            return;
        }

        SelectedStaff = staff;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void UpdateButtonStates()
    {
        ViewHelpers.SetButtonState(_selectButton, _grid.CurrentRow?.DataBoundItem is LinkedStaffOption, AppTheme.DeepGreen);
    }

    private static string NormalizeSearchText(string value)
    {
        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.ToUpperInvariant(character));
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).Replace('Đ', 'D');
    }
}
