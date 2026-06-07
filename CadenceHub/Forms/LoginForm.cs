using CadenceHub.Security;
using CadenceHub.Services;
using CadenceHub.UI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace CadenceHub.Forms;

public sealed class LoginForm : Form
{
    private readonly AuthService _authService = new();
    private readonly TextBox _usernameBox = new();
    private readonly TextBox _passwordBox = new();
    private readonly Label _messageLabel = new();
    private readonly Button _loginButton = new();
    private readonly CheckBox _showPasswordCheckBox = new();

    public LoginForm()
    {
        Text = "CadenceHub - Đăng nhập";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 620);
        Size = new Size(1120, 680);
        BackColor = AppTheme.Page;
        Font = AppTheme.Font(10);
        AppIconProvider.ApplyTo(this);

        BuildLayout();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AuthenticatedUser? AuthenticatedUser { get; private set; }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Page
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        var brandPanel = new BrandPanel { Dock = DockStyle.Fill };
        root.Controls.Add(brandPanel, 0, 0);

        var host = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Page,
            Padding = new Padding(48)
        };
        root.Controls.Add(host, 1, 0);

        var card = BuildLoginCard();
        host.Controls.Add(card);
        host.Resize += (_, _) => CenterCard(host, card);
        CenterCard(host, card);

        Controls.Add(root);
    }

    private RoundedPanel BuildLoginCard()
    {
        var card = new RoundedPanel
        {
            Size = new Size(460, 488),
            Padding = new Padding(34, 30, 34, 30),
            CornerRadius = 8,
            BorderColor = Color.FromArgb(226, 231, 237),
            BackColor = AppTheme.Surface
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 10,
            BackColor = AppTheme.Surface
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "Đăng nhập hệ thống",
            Dock = DockStyle.Fill,
            Font = AppTheme.Font(20, FontStyle.Bold),
            ForeColor = AppTheme.Navy,
            TextAlign = ContentAlignment.MiddleLeft
        };
        layout.Controls.Add(title, 0, 0);

        var subtitle = new Label
        {
            Text = "Xác thực tài khoản để truy cập chức năng theo phân quyền.",
            Dock = DockStyle.Fill,
            Font = AppTheme.Font(10.5f),
            ForeColor = AppTheme.MutedText,
            TextAlign = ContentAlignment.TopLeft
        };
        layout.Controls.Add(subtitle, 0, 1);

        layout.Controls.Add(BuildFieldLabel("Tài khoản"), 0, 2);
        ConfigureTextBox(_usernameBox, "Nhập tên đăng nhập");
        layout.Controls.Add(_usernameBox, 0, 3);

        layout.Controls.Add(BuildFieldLabel("Mật khẩu"), 0, 5);
        ConfigureTextBox(_passwordBox, "Nhập mật khẩu");
        _passwordBox.UseSystemPasswordChar = true;
        _passwordBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _ = LoginAsync();
            }
        };
        layout.Controls.Add(_passwordBox, 0, 6);

        _showPasswordCheckBox.Text = "Hiển thị mật khẩu";
        _showPasswordCheckBox.Dock = DockStyle.Fill;
        _showPasswordCheckBox.ForeColor = AppTheme.MutedText;
        _showPasswordCheckBox.CheckedChanged += (_, _) =>
        {
            _passwordBox.UseSystemPasswordChar = !_showPasswordCheckBox.Checked;
        };
        layout.Controls.Add(_showPasswordCheckBox, 0, 7);

        _loginButton.Text = "Đăng nhập";
        _loginButton.Dock = DockStyle.Fill;
        _loginButton.FlatStyle = FlatStyle.Flat;
        _loginButton.FlatAppearance.BorderSize = 0;
        _loginButton.BackColor = AppTheme.PoliceRed;
        _loginButton.ForeColor = Color.White;
        _loginButton.Font = AppTheme.Font(11.5f, FontStyle.Bold);
        _loginButton.Cursor = Cursors.Hand;
        _loginButton.Click += async (_, _) => await LoginAsync();
        layout.Controls.Add(_loginButton, 0, 8);

        _messageLabel.Dock = DockStyle.Fill;
        _messageLabel.ForeColor = AppTheme.Danger;
        _messageLabel.Font = AppTheme.Font(9.5f, FontStyle.Bold);
        _messageLabel.TextAlign = ContentAlignment.TopLeft;
        layout.Controls.Add(_messageLabel, 0, 9);

        card.Controls.Add(layout);
        return card;
    }

    private static Label BuildFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = AppTheme.Font(10.5f, FontStyle.Bold),
            ForeColor = AppTheme.Ink,
            TextAlign = ContentAlignment.BottomLeft
        };
    }

    private static void ConfigureTextBox(TextBox textBox, string placeholder)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = AppTheme.Font(11.5f);
        textBox.ForeColor = AppTheme.Ink;
        textBox.PlaceholderText = placeholder;
        textBox.Margin = new Padding(0, 0, 0, 0);
    }

    private static void CenterCard(Control host, Control card)
    {
        card.Left = Math.Max(24, (host.ClientSize.Width - card.Width) / 2);
        card.Top = Math.Max(24, (host.ClientSize.Height - card.Height) / 2);
    }

    private async Task LoginAsync()
    {
        _messageLabel.Text = string.Empty;
        SetBusy(true);

        try
        {
            var result = await _authService.AuthenticateAsync(_usernameBox.Text, _passwordBox.Text);
            if (!result.Succeeded || result.User is null)
            {
                _messageLabel.Text = result.Message;
                return;
            }

            AuthenticatedUser = result.User;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (SqlException)
        {
            _messageLabel.Text = "Không kết nối được cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối.";
        }
        catch (DbUpdateException)
        {
            _messageLabel.Text = "Không cập nhật được phiên đăng nhập. Vui lòng kiểm tra quyền ghi dữ liệu.";
        }
        catch (InvalidOperationException)
        {
            _messageLabel.Text = "Dữ liệu tài khoản chưa hợp lệ. Vui lòng kiểm tra bảng người dùng và vai trò.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        _loginButton.Enabled = !isBusy;
        _usernameBox.Enabled = !isBusy;
        _passwordBox.Enabled = !isBusy;
        _showPasswordCheckBox.Enabled = !isBusy;
        _loginButton.Text = isBusy ? "Đang xác thực..." : "Đăng nhập";
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
    }
}
