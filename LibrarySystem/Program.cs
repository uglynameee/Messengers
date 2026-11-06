using System;
using System.Windows.Forms;

namespace LibrarySystem
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                AccessDatabase.Initialize();
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() == DialogResult.OK)
                        Application.Run(new MainForm(login.UserName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось инициализировать базу Microsoft Access.\n\nПуть: " + AccessDatabase.DatabasePathForDisplay + "\n\nПодробности: " + ex.Message,
                    "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
