namespace CadenceHub.UI;

public static class LogoProvider
{
    public static Image? LoadLogo()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
        if (!File.Exists(path))
        {
            return null;
        }

        using var source = Image.FromFile(path);
        return new Bitmap(source);
    }
}
