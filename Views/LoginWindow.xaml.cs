using System.Windows;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Services;

namespace TPV_Gastronomico.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text.Trim();
            var password = PasswordBox.Password;

            using (var db = new DatabaseContext())
            {
                var authService = new AuthService(db);
                var user = authService.Authenticate(username, password);

                if (user == null)
                {
                    StatusText.Text = "Kredentzial okerrak.";
                    return;
                }

                // Abrir MainWindow pasando el usuario logeado
                var main = new MainWindow(user);
                main.Show();
                this.Close();
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
