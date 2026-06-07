using System.Drawing.Drawing2D;

namespace CadenceHub.UI;

public sealed class BrandPanel : Panel
{
    private readonly Image? _logo;

    public BrandPanel()
    {
        DoubleBuffered = true;
        ForeColor = Color.White;
        Padding = new Padding(40);
        _logo = LogoProvider.LoadLogo();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var background = new LinearGradientBrush(
            ClientRectangle,
            AppTheme.PoliceRedDark,
            AppTheme.PoliceRed,
            LinearGradientMode.ForwardDiagonal);
        e.Graphics.FillRectangle(background, ClientRectangle);

        DrawGoldBand(e.Graphics);
        DrawLogo(e.Graphics);
        DrawBrandText(e.Graphics);
    }

    private void DrawGoldBand(Graphics graphics)
    {
        using var brush = new SolidBrush(Color.FromArgb(42, AppTheme.Gold));
        graphics.FillRectangle(brush, 0, Height - 94, Width, 94);

        using var pen = new Pen(Color.FromArgb(175, AppTheme.Gold), 2);
        graphics.DrawLine(pen, 40, Height - 94, Math.Max(40, Width - 40), Height - 94);
    }

    private void DrawLogo(Graphics graphics)
    {
        if (_logo is null)
        {
            return;
        }

        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var logoSize = Math.Min(148, Math.Max(116, Width / 3));
        var x = Math.Max(44, (Width - logoSize) / 2);
        var bounds = new Rectangle(x, 58, logoSize, logoSize);

        graphics.DrawImage(_logo, bounds);
    }

    private void DrawBrandText(Graphics graphics)
    {
        var titleRect = new Rectangle(44, 226, Math.Max(220, Width - 88), 60);
        TextRenderer.DrawText(
            graphics,
            "CADENCEHUB",
            AppTheme.Font(26, FontStyle.Bold),
            titleRect,
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var subtitleRect = new Rectangle(48, 292, Math.Max(220, Width - 96), 86);
        TextRenderer.DrawText(
            graphics,
            "Hệ thống quản lý điểm danh cán bộ nội bộ",
            AppTheme.Font(13, FontStyle.Regular),
            subtitleRect,
            Color.FromArgb(245, 248, 250),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak);

        var ruleRect = new Rectangle(46, Height - 70, Math.Max(220, Width - 92), 46);
        TextRenderer.DrawText(
            graphics,
            "Bảo mật vai trò  |  Dữ liệu nội bộ  |  Vận hành hằng ngày",
            AppTheme.Font(10.5f, FontStyle.Bold),
            ruleRect,
            AppTheme.GoldSoft,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logo?.Dispose();
        }

        base.Dispose(disposing);
    }
}
