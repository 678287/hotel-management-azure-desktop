using System.Windows;
using HotelManagement.Desktop.Views;
using HotelManagement.Shared.Data;
using HotelManagement.Shared.Services;

namespace HotelManagement.Desktop
{
    public partial class App : Application
    {
        public static HotelDbContext DbContext { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dbContext = DbContextFactory.Create(); // or however you instantiate it
            var loginService = new LoginService(dbContext);
            var loginWindow = new LoginWindow(loginService);

            // Open the LoginWindow as a modal dialog
            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                // Proceed to the main application window
                if (Application.Current != null && !Application.Current.Dispatcher.HasShutdownStarted)
                {
                    var mainWindow = new MainWindow(loginService);
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                }
            }
            else
            {
                // Handle login failure or cancellation
                Shutdown();
            }
        }


    }
}