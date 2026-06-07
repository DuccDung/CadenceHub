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
        _grid.ReadOnly = true;
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
        var selectButton = ViewHelpers.CommandButton("Chọn", AppTheme.DeepGreen);
        selectButton.Click += (_, _) => ConfirmSelection();
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(selectButton);
        root.Controls.Add(buttons, 0, 2);

        AcceptButton = selectButton;
        CancelButton = cancelButton;
        Controls.Add(root);
    }

    private void ApplyFilter()
    {
        var query = NormalizeSearchText(_searchBox.Text);
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allStaff.ToList()
            : _allStaff
                .Where(staff => NormalizeSearchText($"{staff.StaffCode} {staff.FullName} {staff.Unit} {staff.Usernames}").Contains(query, StringComparison.Ordinal))
                .ToList();

        _grid.DataSource = null;
        _grid.DataSource = filtered;
        ConfigureGridColumns();
        SelectRowById(_initialStaffId);
    }

    private void ConfigureGridColumns()
    {
        HideColumn(nameof(LinkedStaffOption.Id));
        HideColumn(nameof(LinkedStaffOption.DisplayName));
        ConfigureColumn(nameof(LinkedStaffOption.StaffCode), "Mã cán bộ", 110);
        ConfigureColumn(nameof(LinkedStaffOption.FullName), "Họ tên", 230);
        ConfigureColumn(nameof(LinkedStaffOption.Unit), "Đơn vị", 120);
        ConfigureColumn(nameof(LinkedStaffOption.Usernames), "Tài khoản", 170);

        if (_grid.Columns[nameof(LinkedStaffOption.FullName)] is { } nameColumn)
        {
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
    }

    private void ConfigureColumn(string name, string header, int width)
    {
        if (_grid.Columns[name] is not { } column)
        {
            return;
        }

        column.HeaderText = header;
        column.MinimumWidth = width;
        column.Width = width;
    }

    private void HideColumn(string name)
    {
        if (_grid.Columns[name] is { } column)
        {
            column.Visible = false;
        }
    }

    private void SelectRowById(int staffId)
    {
        if (staffId <= 0)
        {
            return;
        }

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is not LinkedStaffOption staff || staff.Id != staffId)
            {
                continue;
            }

            row.Selected = true;
            _grid.CurrentCell = row.Cells[nameof(LinkedStaffOption.FullName)];
            return;
        }
    }

    private void ConfirmSelection()
    {
        if (_grid.CurrentRow?.DataBoundItem is not LinkedStaffOption staff)
        {
            return;
        }

        SelectedStaff = staff;
        DialogResult = DialogResult.OK;
        Close();
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
