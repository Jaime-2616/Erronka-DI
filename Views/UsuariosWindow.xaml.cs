using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Services;

namespace TPV_Gastronomico.Views
{
    public partial class UsuariosWindow : Window
    {
        private readonly UsuarioService _usuarioService = new UsuarioService();
        private User _selectedUser;

        public UsuariosWindow()
        {
            InitializeComponent();
            LoadAll();
            RolCombo.SelectedIndex = 1; // por defecto Usuario
        }

        private void LoadAll()
        {
            try
            {
                var list = _usuarioService.GetAll().ToList();
                UsuariosGrid.ItemsSource = list;
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando usuarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refrescar_Click(object sender, RoutedEventArgs e) => LoadAll();

        private void Buscar_Click(object sender, RoutedEventArgs e)
        {
            var term = SearchBox.Text?.Trim().ToLower();
            var list = _usuarioService.GetAll()
                        .Where(u => string.IsNullOrWhiteSpace(term) ||
                                    u.Nombre.ToLower().Contains(term) ||
                                    u.Username.ToLower().Contains(term))
                        .ToList();
            UsuariosGrid.ItemsSource = list;
            ClearForm();
        }

        private void UsuariosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = UsuariosGrid.SelectedItem as User;
            if (_selectedUser != null)
            {
                NombreBox.Text = _selectedUser.Nombre;
                UsernameBox.Text = _selectedUser.Username;
                // No ponemos la contraseña (vacío), para cambiarla hay que introducir una nueva
                PasswordBox.Password = "";
                RolCombo.SelectedIndex = _selectedUser.Rol == Role.Admin ? 0 : 1;
            }
        }

        private void ClearForm()
        {
            _selectedUser = null;
            NombreBox.Text = "";
            UsernameBox.Text = "";
            PasswordBox.Password = "";
            RolCombo.SelectedIndex = 1;
            UsuariosGrid.UnselectAll();
        }

        private void Anadir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nombre = NombreBox.Text.Trim();
                var username = UsernameBox.Text.Trim();
                var pwd = PasswordBox.Password;
                var rol = RolCombo.SelectedIndex == 0 ? Role.Admin : Role.Usuario;

                var user = new User
                {
                    Nombre = nombre,
                    Username = username,
                    Rol = rol
                };

                _usuarioService.Add(user, pwd);
                LoadAll();
                MessageBox.Show("Usuario añadido correctamente.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo añadir el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Selecciona un usuario para actualizar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _selectedUser.Nombre = NombreBox.Text.Trim();
                _selectedUser.Username = UsernameBox.Text.Trim();
                var newPwd = PasswordBox.Password;
                _selectedUser.Rol = RolCombo.SelectedIndex == 0 ? Role.Admin : Role.Usuario;

                _usuarioService.Update(_selectedUser, string.IsNullOrWhiteSpace(newPwd) ? null : newPwd);
                LoadAll();
                MessageBox.Show("Usuario actualizado correctamente.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo actualizar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Selecciona un usuario para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"¿Eliminar al usuario '{_selectedUser.Nombre}' ({_selectedUser.Username})?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _usuarioService.Delete(_selectedUser.Id);
                LoadAll();
                MessageBox.Show("Usuario eliminado.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
