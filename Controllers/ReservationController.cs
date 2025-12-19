using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Services;
using TPV_Gastronomico.Views;

namespace TPV_Gastronomico.Controllers
{
    public class ReservationController
    {
        private readonly ReservaService _reservaService;
        private readonly MesaService _mesaService;
        private readonly User _usuario;
        private readonly ReservationControl _view;

        public event EventHandler<Reserva> ReservaCreated;
        public event EventHandler Cancelled;

        public ReservationController(ReservationControl view, User usuario)
        {
            // Euskera: Kontroladorea hasieratu; ikuspegia eta erabiltzailea gorde.
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            _reservaService = new ReservaService(new DatabaseContext());
            _mesaService = new MesaService(new DatabaseContext());

            WireEvents();
            InitializeControls();
        }

        private void WireEvents()
        {
            // Euskera: Ikuspegiko gertaera loturak
            _view.BtnBuscarMesas.Click += BtnBuscarMesas_Click;
            _view.BtnCrearReserva.Click += BtnCrearReserva_Click;
            _view.BtnCancelar.Click += BtnCancelar_Click;
            _view.cbTipoComida.SelectionChanged += CbTipoComida_SelectionChanged;
        }

        private void InitializeControls()
        {
            // Tipo comida
            // Euskera: Hasierako balioak ezarri (mota, data, orduak, minutuak, pertsonen kopurua)
            _view.cbTipoComida.ItemsSource = Enum.GetValues(typeof(TipoComida)).Cast<TipoComida>().ToList();
            _view.cbTipoComida.SelectedIndex = 0;

            // Fecha por defecto: mañana
            _view.dpFecha.SelectedDate = DateTime.Now.AddDays(1);

            // Minutos
            _view.cbMinuto.Items.Clear();
            var minutes = new[] { "00", "15", "30", "45" };
            foreach (var m in minutes) _view.cbMinuto.Items.Add(m);
            _view.cbMinuto.SelectedIndex = 0;

            _view.txtNumPersonas.Text = "2";

            // Horas iniciales basadas en tipo
            PopulateHoursForTipo((TipoComida)_view.cbTipoComida.SelectedItem);
        }

        private void CbTipoComida_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_view.cbTipoComida.SelectedItem is TipoComida tipo)
                PopulateHoursForTipo(tipo);
        }

        private void PopulateHoursForTipo(TipoComida tipo)
        {
            // Euskera: Mota bakoitzerako ordutegi posibleak betetzen ditu (desayuno, almuerzo, cena)
            _view.cbHora.Items.Clear();
            switch (tipo)
            {
                case TipoComida.Desayuno:
                    _view.cbHora.Items.Add("09");
                    _view.cbHora.Items.Add("10");
                    break;
                case TipoComida.Almuerzo:
                    _view.cbHora.Items.Add("13");
                    _view.cbHora.Items.Add("14");
                    break;
                case TipoComida.Cena:
                    _view.cbHora.Items.Add("20");
                    _view.cbHora.Items.Add("21");
                    break;
                default:
                    _view.cbHora.Items.Add("12");
                    _view.cbHora.Items.Add("13");
                    break;
            }
            _view.cbHora.SelectedIndex = 0;
        }

        private void BtnBuscarMesas_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Euskera: Bilatu mahaiak emandako datetarako eta eguneratu ikuspegia
            if (!ValidarDatosReserva()) return;

            var fecha = ObtenerFechaCompleta();
            var tipoComida = (TipoComida)_view.cbTipoComida.SelectedItem;
            if (!int.TryParse(_view.txtNumPersonas.Text, out int numPersonas)) numPersonas = 1;

            var mesasDisponibles = _reservaService.GetMesasDisponibles(fecha, tipoComida, numPersonas);
            _view.cbMesa.ItemsSource = mesasDisponibles;
            _view.dgMesasDisponibles.ItemsSource = mesasDisponibles;

            if (mesasDisponibles.Any())
            {
                _view.cbMesa.SelectedIndex = 0;
                _view.txtInfoMesa.Text = $"{mesasDisponibles.Count} mesas disponibles";
                _view.txtInfoMesa.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                _view.txtInfoMesa.Text = "No hay mesas disponibles";
                _view.txtInfoMesa.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private DateTime ObtenerFechaCompleta()
        {
            // Euskera: DatePicker eta ordu aukeraketatik dataren string osoa sortu eta parse egin
            var fecha = _view.dpFecha.SelectedDate ?? DateTime.Now.Date;
            var hora = _view.cbHora.SelectedItem?.ToString() ?? "12";
            var minuto = _view.cbMinuto.SelectedItem?.ToString() ?? "00";
            var s = $"{fecha:yyyy-MM-dd} {hora}:{minuto}";
            return DateTime.Parse(s);
        }

        private bool ValidarDatosReserva()
        {
            // Euskera: Sarrera balidazioak (data, pertsonak)
            if (_view.dpFecha.SelectedDate == null)
            {
                MessageBox.Show("Seleccione una fecha", "Fecha requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_view.dpFecha.SelectedDate.Value < DateTime.Now.Date)
            {
                MessageBox.Show("La fecha no puede ser anterior a hoy", "Fecha inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(_view.txtNumPersonas.Text, out int numPersonas) || numPersonas < 1 || numPersonas > 20)
            {
                MessageBox.Show("Número de personas no válido (1-20)", "Dato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnCrearReserva_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidarDatosReserva()) return;
            if (_view.cbMesa.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una mesa disponible", "Mesa requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var reserva = new Reserva
                {
                    MesaId = (_view.cbMesa.SelectedItem as Mesa).Id,
                    UsuarioId = _usuario.Id,
                    Fecha = ObtenerFechaCompleta(),
                    Tipo = (TipoComida)_view.cbTipoComida.SelectedItem,
                    NumPersonas = int.Parse(_view.txtNumPersonas.Text)
                };

                if (_reservaService.CrearReserva(reserva))
                {
                    ReservaCreated?.Invoke(this, reserva);
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

        private void BtnCancelar_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}