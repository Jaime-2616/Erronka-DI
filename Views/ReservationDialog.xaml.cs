using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Services;

namespace TPV_Gastronomico.Views
{
    public partial class ReservationDialog : Window
    {
        private readonly ReservaService _reservaService;
        private readonly MesaService _mesaService;
        private readonly User _usuario;

        public Reserva CreatedReserva { get; private set; }

        public ReservationDialog(User usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            _reservaService = new ReservaService(new DatabaseContext());
            _mesaService = new MesaService(new DatabaseContext());

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Tipo comida
            cbTipoComida.ItemsSource = Enum.GetValues(typeof(TipoComida)).Cast<TipoComida>().ToList();
            cbTipoComida.SelectedIndex = 0;

            // Hook para actualizar horas según tipo seleccionado
            cbTipoComida.SelectionChanged += CbTipoComida_SelectionChanged;

            // Fecha por defecto: mañana
            dpFecha.SelectedDate = DateTime.Now.AddDays(1);

            // Horas: solo 6 opciones (2 por cada tipo)
            PopulateHoursForTipo((TipoComida)cbTipoComida.SelectedItem);

            // Minutos (00,15,30,45)
            var minutes = new[] { "00", "15", "30", "45" };
            cbMinuto.Items.Clear();
            foreach (var m in minutes) cbMinuto.Items.Add(m);
            cbMinuto.SelectedIndex = 0;

            txtNumPersonas.Text = "2";
        }

        private void CbTipoComida_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTipoComida.SelectedItem is TipoComida tipo)
            {
                PopulateHoursForTipo(tipo);
            }
        }

        private void PopulateHoursForTipo(TipoComida tipo)
        {
            cbHora.Items.Clear();

            // Dos opciones por tipo: ajustar a los horarios que necesites
            switch (tipo)
            {
                case TipoComida.Desayuno:
                    cbHora.Items.Add("09"); // 09:00
                    cbHora.Items.Add("10"); // 10:00
                    break;
                case TipoComida.Almuerzo:
                    cbHora.Items.Add("13"); // 13:00
                    cbHora.Items.Add("14"); // 14:00
                    break;
                case TipoComida.Cena:
                    cbHora.Items.Add("20"); // 20:00
                    cbHora.Items.Add("21"); // 21:00
                    break;
                default:
                    cbHora.Items.Add("12");
                    cbHora.Items.Add("13");
                    break;
            }

            cbHora.SelectedIndex = 0;
        }

        private void BtnBuscarMesas_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatosReserva()) return;

            var fecha = ObtenerFechaCompleta();
            var tipoComida = (TipoComida)cbTipoComida.SelectedItem;
            if (!int.TryParse(txtNumPersonas.Text, out int numPersonas)) numPersonas = 1;

            var mesasDisponibles = _reservaService.GetMesasDisponibles(fecha, tipoComida, numPersonas);
            cbMesa.ItemsSource = mesasDisponibles;
            dgMesasDisponibles.ItemsSource = mesasDisponibles;

            if (mesasDisponibles.Any())
            {
                cbMesa.SelectedIndex = 0;
                txtInfoMesa.Text = $"{mesasDisponibles.Count} mesas disponibles";
                txtInfoMesa.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                txtInfoMesa.Text = "No hay mesas disponibles";
                txtInfoMesa.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private DateTime ObtenerFechaCompleta()
        {
            var fecha = dpFecha.SelectedDate ?? DateTime.Now.Date;
            var hora = cbHora.SelectedItem?.ToString() ?? "12";
            var minuto = cbMinuto.SelectedItem?.ToString() ?? "00";
            var s = $"{fecha:yyyy-MM-dd} {hora}:{minuto}";
            return DateTime.Parse(s);
        }

        private bool ValidarDatosReserva()
        {
            if (dpFecha.SelectedDate == null)
            {
                MessageBox.Show("Seleccione una fecha", "Fecha requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dpFecha.SelectedDate.Value < DateTime.Now.Date)
            {
                MessageBox.Show("La fecha no puede ser anterior a hoy", "Fecha inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(txtNumPersonas.Text, out int numPersonas) || numPersonas < 1 || numPersonas > 20)
            {
                MessageBox.Show("Número de personas no válido (1-20)", "Dato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnCrearReserva_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatosReserva()) return;
            if (cbMesa.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una mesa disponible", "Mesa requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var reserva = new Reserva
                {
                    MesaId = (cbMesa.SelectedItem as Mesa).Id,
                    UsuarioId = _usuario.Id,
                    Fecha = ObtenerFechaCompleta(),
                    Tipo = (TipoComida)cbTipoComida.SelectedItem,
                    NumPersonas = int.Parse(txtNumPersonas.Text)
                };

                if (_reservaService.CrearReserva(reserva))
                {
                    CreatedReserva = reserva;
                    MessageBox.Show("Reserva creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("No se pudo crear la reserva", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creando reserva: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}