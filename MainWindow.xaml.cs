using System.Windows;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Views;

namespace TPV_Gastronomico
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            // Mostrar bienvenida
            WelcomeText.Text = $"Kaixo, {_currentUser.Nombre} ({_currentUser.Rol})";

            // Ocultar botones de admin si no es admin
            if (_currentUser.Rol == Role.Admin)
            {
                BtnUsuarios.Visibility = Visibility.Visible;
                BtnStock.Visibility = Visibility.Visible;
                BtnReservas.Visibility = Visibility.Visible; // Admin también ve reservas
                BtnTickets.Visibility = Visibility.Visible;  // Admin también ve tickets
            }
            else if (_currentUser.Rol == Role.Usuario)
            {
                BtnUsuarios.Visibility = Visibility.Collapsed;
                BtnStock.Visibility = Visibility.Collapsed;
                BtnReservas.Visibility = Visibility.Visible;
                BtnTickets.Visibility = Visibility.Visible;
            }
        }

        // Botón Usuarios (solo admin)
        private void BtnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            var ventanaUsuarios = new Views.UsuariosWindow();
            ventanaUsuarios.ShowDialog();
        }

        // Botón Stock
        private void BtnStock_Click(object sender, RoutedEventArgs e)
        {
            var ventanaStock = new Views.StockWindow();
            ventanaStock.ShowDialog();
        }

        // Botón Reservas
        private void BtnReservas_Click(object sender, RoutedEventArgs e)
        {
            var ventanaReservas = new Views.ReservasWindow(_currentUser);
            ventanaReservas.ShowDialog();
        }

        // Botón Tickets
        private void BtnTickets_Click(object sender, RoutedEventArgs e)
        {
            //var ventanaTickets = new Views.TicketWindow();
            //ventanaTickets.ShowDialog();

        }
    }
}