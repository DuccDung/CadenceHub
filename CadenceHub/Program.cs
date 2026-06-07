namespace CadenceHub
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.SetDefaultFont(new Font("Segoe UI", 10F));

            while (true)
            {
                using var loginForm = new Forms.LoginForm();
                if (loginForm.ShowDialog() != DialogResult.OK || loginForm.AuthenticatedUser is null)
                {
                    break;
                }

                using var mainForm = new Forms.MainForm(loginForm.AuthenticatedUser);
                Application.Run(mainForm);

                if (!mainForm.LogoutRequested)
                {
                    break;
                }
            }
        }
    }
}
