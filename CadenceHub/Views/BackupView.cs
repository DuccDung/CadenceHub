using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;
using System.Diagnostics;

namespace CadenceHub.Views;

public sealed class BackupView : UserControl
{
    private readonly ExcelExportService _exportService = new();
    private readonly BusinessDataService _dataService = new();
    private readonly AuthenticatedUser _actor;
    private readonly Label _resultLabel = new();
    private readonly Button _backupButton = ViewHelpers.CommandButton("Tạo bản sao lưu");
    private readonly Button _openButton = ViewHelpers.CommandButton("Mở thư mục backup", AppTheme.DeepGreen);
    private bool _isBusy;

    public BackupView(AuthenticatedUser actor)
    {
        _actor = actor;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Page;
        BuildLayout();
    }

    private void BuildLayout()
    {
        var card = ViewHelpers.Card(new Padding(28));
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 220, RowCount = 4, ColumnCount = 1, BackColor = AppTheme.Surface };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(ViewHelpers.Title("Sao lưu dữ liệu vận hành"), 0, 0);
        layout.Controls.Add(ViewHelpers.InfoLabel("Bản sao lưu hiện xuất ra file Excel gồm dữ liệu nền, lịch trực, điểm danh, cấu hình và nhật ký."), 0, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = AppTheme.Surface };
        _backupButton.Click += async (_, _) => await BackupAsync();
        _openButton.Click += async (_, _) => await OpenBackupFolderAsync();
        buttons.Controls.AddRange([_backupButton, _openButton]);
        layout.Controls.Add(buttons, 0, 2);

        _resultLabel.Dock = DockStyle.Fill;
        _resultLabel.Font = AppTheme.Font(10.5f, FontStyle.Bold);
        _resultLabel.ForeColor = AppTheme.DeepGreen;
        layout.Controls.Add(_resultLabel, 0, 3);
        card.Controls.Add(layout);
        Controls.Add(card);
        UpdateButtonStates();
    }

    private async Task BackupAsync()
    {
        try
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            UpdateButtonStates();

            var path = await _exportService.CreateFullBackupWorkbookAsync(_actor);
            _resultLabel.Text = path;
            ViewHelpers.ShowInfo(this, $"Đã tạo bản sao lưu:\r\n{path}");
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

    private async Task OpenBackupFolderAsync()
    {
        try
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            UpdateButtonStates();

            var directory = await _dataService.GetBackupDirectoryAsync();
            Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
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
        ViewHelpers.SetButtonState(_backupButton, !_isBusy, AppTheme.PoliceRed);
        ViewHelpers.SetButtonState(_openButton, !_isBusy, AppTheme.DeepGreen);
    }
}
