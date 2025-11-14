using System;
using System.Windows.Forms;
using YourChatApp.Client.Forms;

namespace YourChatApp.Client
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                Console.WriteLine("╔════════════════════════════════════════════════╗");
                Console.WriteLine("║        YourChatApp Client                      ║");
                Console.WriteLine("║        Version 1.0                             ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");

                LoginForm loginForm = new LoginForm();
                Application.Run(loginForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal error: {ex.Message}");
                Console.WriteLine($"[FATAL] {ex}");
            }
        }
    }
}
