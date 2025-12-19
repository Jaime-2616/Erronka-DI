using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Services;
using TPV_Gastronomico.Data;

namespace TPV_Gastronomico.Views
{
    public partial class ReservasWindow : Window
    {
        private readonly ReservaService _reservaService;
        private readonly MesaService _mesaService;
        private User _usuarioActual;

        public ReservasWindow(User usuario)
        {
            InitializeComponent();
            _reservaService = new ReservaService(new DatabaseContext());
            _mesaService = new MesaService(new DatabaseContext());
            _usuarioActual = usuario;

            InicializarControles();
            CargarReservas();
        }

        private void InicializarControles()
        {
            // Inicializar combo de tipo de comida
            cbTipoComida.ItemsSource = Enum.GetValues(typeof(TipoComida))
                .Cast<TipoComida>()
                .ToDictionary(t => t, t => t.ToString());
            cbTipoComida.SelectedIndex = 0;

            // Establecer fecha por defecto (mañana)
            dpFecha.SelectedDate = DateTime.Now.AddDays(1);

            // Configurar título según rol
            if (_usuarioActual.Rol == Role.Admin)
            {
                txtTitulo.Text = "Vista de Administrador - Todas las reservas";
                this.Title = "Gestión de Todas las Reservas (Admin)";
            }
            else
            {
                txtTitulo.Text = $"Reservas de {_usuarioActual.Nombre}";
                this.Title = "Mis Reservas";
            }
        }

        private void CargarReservas()
        {
            if (_usuarioActual.Rol == Role.Admin)
            {
                dgMisReservas.ItemsSource = _reservaService.GetAllReservas();
            }
            else
            {
                dgMisReservas.ItemsSource = _reservaService.GetReservasByUsuario(_usuarioActual.Id);
            }
        }

        private void BtnBuscarMesas_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatosReserva())
                return;

            var fecha = ObtenerFechaCompleta();
            var tipoComida = (TipoComida)cbTipoComida.SelectedValue;
            var numPersonas = int.Parse(txtNumPersonas.Text);

            var mesasDisponibles = _reservaService.GetMesasDisponibles(fecha, tipoComida, numPersonas);
            cbMesa.ItemsSource = mesasDisponibles;
            dgMesasDisponibles.ItemsSource = mesasDisponibles;

            if (mesasDisponibles.Any())
            {
                cbMesa.SelectedIndex = 0;
                txtInfoMesa.Text = $"{mesasDisponibles.Count} mesas disponibles para {numPersonas} personas";
                txtInfoMesa.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                txtInfoMesa.Text = "No hay mesas disponibles para los criterios seleccionados";
                txtInfoMesa.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void BtnCancelarReserva_Click(object sender, RoutedEventArgs e)
        {
            var reserva = dgMisReservas.SelectedItem as Reserva;
            if (reserva != null)
            {
                // Verificar si el usuario puede cancelar esta reserva
                if (_usuarioActual.Rol != Role.Admin && reserva.UsuarioId != _usuarioActual.Id)
                {
                    MessageBox.Show("No tiene permisos para cancelar esta reserva.", "Permiso denegado",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"¿Está seguro de que desea cancelar la reserva de la Mesa {reserva.Mesa.Numero} para el {reserva.Fecha:dd/MM/yyyy} a las {reserva.Fecha:HH:mm}?",
                    "Confirmar cancelación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_reservaService.CancelarReserva(reserva.Id))
                    {
                        MessageBox.Show("✅ Reserva cancelada exitosamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarReservas();
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione una reserva para cancelar", "Selección requerida",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarReservas();
            MessageBox.Show("Lista de reservas actualizada", "Actualizado",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCrearReserva_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ReservationDialogWindow(_usuarioActual) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                MessageBox.Show("✅ Reserva creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarReservas();
                LimpiarFormulario();
            }
        }

        private DateTime ObtenerFechaCompleta()
        {
            var fecha = dpFecha.SelectedDate.Value;
            var horaSeleccionada = (cbHora.SelectedItem as ComboBoxItem).Content.ToString();
            var minutoSeleccionado = (cbMinuto.SelectedItem as ComboBoxItem).Content.ToString();

            return DateTime.Parse($"{fecha:yyyy-MM-dd} {horaSeleccionada}:{minutoSeleccionado}");
        }

        private bool ValidarDatosReserva()
        {
            if (dpFecha.SelectedDate == null)
            {
                MessageBox.Show("Seleccione una fecha", "Fecha requerida",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpFecha.SelectedDate.Value < DateTime.Now.Date)
            {
                MessageBox.Show("La fecha no puede ser anterior a hoy", "Fecha inválida",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(txtNumPersonas.Text, out int numPersonas) || numPersonas < 1 || numPersonas > 20)
            {
                MessageBox.Show("Número de personas no válido (1-20)", "Dato inválido",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            txtNumPersonas.Text = "2";
            cbMesa.ItemsSource = null;
            dgMesasDisponibles.ItemsSource = null;
            txtInfoMesa.Text = "";
            dpFecha.SelectedDate = DateTime.Now.AddDays(1);
            cbTipoComida.SelectedIndex = 0;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}