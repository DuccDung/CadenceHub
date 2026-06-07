using System.ComponentModel;

namespace CadenceHub.UI;

public sealed class RoundedPanel : Panel
{
    public RoundedPanel()
    {
        DoubleBuffered = true;
        BackColor = AppTheme.Surface;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 8;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = AppTheme.Border;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BorderWidth { get; set; } = 1;

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);

        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        using var path = AppTheme.RoundedRectangle(new Rectangle(0, 0, Width, Height), CornerRadius);
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = AppTheme.RoundedRectangle(rect, CornerRadius);
        using var pen = new Pen(BorderColor, BorderWidth);
        e.Graphics.DrawPath(pen, path);
    }
}
