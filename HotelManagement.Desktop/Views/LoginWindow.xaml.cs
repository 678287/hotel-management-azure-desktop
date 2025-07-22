using System.Windows;
using HotelManagement.Shared.Services;

namespace HotelManagement.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginService _loginService;

        public LoginWindow(LoginService loginService)
        {
            InitializeComponent();
            _loginService = loginService;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;

            var user = await _loginService.LoginAsync(email, password);
            if (user != null)
            {
                MessageBox.Show("Login successful!");
                // Proceed to main window
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.");
            }
        }
    }
}