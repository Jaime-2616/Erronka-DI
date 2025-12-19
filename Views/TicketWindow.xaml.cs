using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Services;

namespace TPV_Gastronomico.Views
{
    public partial class TicketWindow : Window
    {
        private readonly User _user;
        private string _lastTicketText = string.Empty;

        public class ProductoSelection
        {
            public Producto Producto { get; }
            public ProductoSelection(Producto p) => Producto = p;
            public string Nombre => Producto.Nombre;
            public decimal Precio => Producto.Precio;
            public int Disponible => Producto.Cantidad;
            public int SelectedQuantity { get; set; }
        }

        private ObservableCollection<ProductoSelection> _items = new();
        private TextBlock _totalText;
        private readonly ReservaService _reservaService;

        public TicketWindow(User user)
        {
            InitializeComponent();

            _totalText = this.FindName("TotalText") as TextBlock;

            _user = user;
            _reservaService = new ReservaService(new DatabaseContext());

            LoadProducts();
            LoadReservas();

            // bind items to the ItemsControl showing boxes
            ProductsPanel.ItemsSource = _items;
            UpdateTotal();

            // Ensure the Generar button is enabled only when a reserva is selected
            cbReservas.SelectionChanged += CbReservas_SelectionChanged;
            UpdateGenerarEnabled();
        }

        private void LoadProducts()
        {
            using var db = new DatabaseContext();
            var list = db.Productos.OrderBy(p => p.Nombre).ToList();
            _items = new ObservableCollection<ProductoSelection>(
                list.Select(p => new ProductoSelection(p))
            );
            ProductsPanel.ItemsSource = _items;
        }

        private void LoadReservas()
        {
            try
            {
                var reservas = _reservaService.GetReservasByUsuario(_user.Id);
                cbReservas.ItemsSource = reservas;
                cbReservas.SelectedIndex = reservas.Any() ? 0 : -1;

                // update button enabled state after loading reservas
                UpdateGenerarEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotal()
        {
            decimal total = _items.Sum(i => i.SelectedQuantity * i.Precio);
            if (_totalText != null)
                _totalText.Text = total.ToString("C");

            // keep ticket preview in sync
            UpdatePreview();
        }

        // enable/disable Generar button based on reserva selection
        private void UpdateGenerarEnabled()
        {
            BtnGenerar.IsEnabled = cbReservas.Items.Count > 0 && cbReservas.SelectedItem != null;
        }

        private void CbReservas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGenerarEnabled();
        }

        // When clicking a product's button, show a small dialog to pick quantity
        // NOTE: the ProductItemControl raises a CLR EventHandler, so use EventArgs here
        private void SelectQuantity_Click(object sender, EventArgs e)
        {
            // control.DataContext is the ProductoSelection item
            if (sender is ProductItemControl control && control.DataContext is ProductoSelection sel)
            {
                var dlg = new QuantityDialog(sel.Disponible, sel.SelectedQuantity) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    sel.SelectedQuantity = dlg.Quantity;
                    ProductsPanel.Items.Refresh();
                    UpdateTotal();
                    UpdatePreview(); // Update preview after selecting quantity
                }
            }
        }

        private void UpdatePreview()
        {
            // Euskera: Aurrebista eraiki hautatutako produktu eta kantitateetatik
            var selected = _items.Where(i => i.SelectedQuantity > 0).ToList();
            if (!selected.Any())
            {
                PreviewBox.Text = string.Empty;
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("------ PREVIA TXARTELA ------");
            foreach (var s in selected)
            {
                sb.AppendLine($"{s.Nombre} x{s.SelectedQuantity} = {(s.SelectedQuantity * s.Precio):C}");
            }
            sb.AppendLine("-----------------------------");
            sb.AppendLine($"GUZTIRA: {_items.Where(i => i.SelectedQuantity > 0).Sum(i => i.SelectedQuantity * i.Precio):C}");

            PreviewBox.Text = sb.ToString();
        }

        private void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            // Euskera: Tiketa sortu, stocka eguneratu eta gorde datu-basera
            // require a selected reservation before generating ticket
            if (cbReservas.Items.Count > 0 && cbReservas.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar una reserva antes de generar el ticket.", "Informaci?n",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = _items.Where(i => i.SelectedQuantity > 0).ToList();
            if (!selected.Any())
            {
                MessageBox.Show("Hautatu gutxienez produktu bat.", "Atenci?n",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new DatabaseContext();
                var ticket = new Ticket
                {
                    UsuarioId = _user.Id,
                    Fecha = DateTime.Now
                };

                foreach (var sel in selected)
                {
                    if (sel.SelectedQuantity > sel.Disponible)
                    {
                        MessageBox.Show($"Stock insuficiente: {sel.Nombre}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    ticket.Detalles.Add(new TicketDetalle
                    {
                        ProductoId = sel.Producto.Id,
                        Cantidad = sel.SelectedQuantity,
                        PrecioUnitario = sel.Precio
                    });

                    var prod = db.Productos.Single(p => p.Id == sel.Producto.Id);
                    prod.Cantidad -= sel.SelectedQuantity;
                }

                ticket.Total = ticket.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                db.Tickets.Add(ticket);
                db.SaveChanges();

                var sb = new StringBuilder();
                sb.AppendLine("------ TXARTELA ------");
                sb.AppendLine($"ID: {ticket.Id}");
                sb.AppendLine($"Erabiltzailea: {_user.Nombre}");
                sb.AppendLine("----------------------");

                foreach (var d in ticket.Detalles)
                {
                    var prod = db.Productos.Single(p => p.Id == d.ProductoId);
                    sb.AppendLine($"{prod.Nombre} x{d.Cantidad} = {(d.Cantidad * d.PrecioUnitario):C}");
                }

                sb.AppendLine("----------------------");
                sb.AppendLine($"GUZTIRA: {ticket.Total:C}");

                _lastTicketText = sb.ToString();
                PreviewBox.Text = _lastTicketText;

                // If user had selected a reservation, remove it after creating the ticket
                if (cbReservas.SelectedItem is Reserva reservaSeleccionada)
                {
                    _reservaService.CancelarReserva(reservaSeleccionada.Id);
                    LoadReservas();
                }

                LoadProducts();
                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_lastTicketText))
            {
                MessageBox.Show("No hay ticket para imprimir.", "Informaci?n",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var doc = BuildFlowDocumentFromText(_lastTicketText);
            var pd = new PrintDialog();
            // To generate a PDF choose "Microsoft Print to PDF" in the printer list when dialog appears.
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idp = doc;
                pd.PrintDocument(idp.DocumentPaginator, "Ticket");
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        private FlowDocument BuildFlowDocumentFromText(string text)
        {
            var doc = new FlowDocument { FontSize = 12, PagePadding = new Thickness(20) };
            foreach (var line in text.Split(Environment.NewLine))
                doc.Blocks.Add(new Paragraph(new Run(line)));
            return doc;
        }
    }
}
