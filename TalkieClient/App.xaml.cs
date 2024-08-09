using System.Windows;
using TalkieClient.Views;

namespace TalkieClient
{
    public partial class App : Application
    {
        public static int CurrentUserId { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var user = loginWindow.LoggedInUser;
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
            }
        }
    }
}
