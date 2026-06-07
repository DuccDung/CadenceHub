using System.Drawing.Drawing2D;

namespace CadenceHub.UI;

public static class AppTheme
{
    public static readonly Color PoliceRed = Color.FromArgb(136, 18, 24);
    public static readonly Color PoliceRedDark = Color.FromArgb(92, 12, 18);
    public static readonly Color Gold = Color.FromArgb(232, 181, 61);
    public static readonly Color GoldSoft = Color.FromArgb(255, 244, 209);
    public static readonly Color DeepGreen = Color.FromArgb(18, 82, 62);
    public static readonly Color Navy = Color.FromArgb(20, 38, 54);
    public static readonly Color Ink = Color.FromArgb(29, 35, 43);
    public static readonly Color MutedText = Color.FromArgb(93, 101, 113);
    public static readonly Color Border = Color.FromArgb(214, 219, 226);
    public static readonly Color Surface = Color.FromArgb(255, 253, 248);
    public static readonly Color Page = Color.FromArgb(246, 248, 251);
    public static readonly Color Success = Color.FromArgb(21, 128, 91);
    public static readonly Color Warning = Color.FromArgb(180, 112, 20);
    public static readonly Color Danger = Color.FromArgb(185, 28, 28);

    public static Font Font(float size, FontStyle style = FontStyle.Regular)
    {
        return new Font("Segoe UI", size, style, GraphicsUnit.Point);
    }

    public static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var rect = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(rect, 180, 90);
        rect.X = bounds.Right - diameter;
        path.AddArc(rect, 270, 90);
        rect.Y = bounds.Bottom - diameter;
        path.AddArc(rect, 0, 90);
        rect.X = bounds.Left;
        path.AddArc(rect, 90, 90);
        path.CloseFigure();

        return path;
    }
}
