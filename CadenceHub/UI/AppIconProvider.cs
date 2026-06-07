namespace CadenceHub.UI;

public static class AppIconProvider
{
    public static Icon? LoadIcon()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.ico");
        return File.Exists(path) ? new Icon(path) : null;
    }

    public static void ApplyTo(Form form)
    {
        var icon = LoadIcon();
        if (icon is not null)
        {
            form.Icon = icon;
        }
    }
}
