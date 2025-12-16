using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Printing;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;

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

        public TicketWindow(User user)
        {
            InitializeComponent();

            _totalText = this.FindName("TotalText") as TextBlock;

            _user = user;
            LoadProducts();
            ProductosGrid.ItemsSource = _items;
            UpdateTotal();

            ProductosGrid.CellEditEnding += (s, e) => UpdateTotal();
        }

        private void LoadProducts()
        {
            using var db = new DatabaseContext();
            var list = db.Productos.OrderBy(p => p.Nombre).ToList();
            _items = new ObservableCollection<ProductoSelection>(
                list.Select(p => new ProductoSelection(p))
            );
            ProductosGrid.ItemsSource = _items;
        }

        private void UpdateTotal()
        {
            decimal total = _items.Sum(i => i.SelectedQuantity * i.Precio);
            _totalText.Text = total.ToString("C");
        }

        private void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            var selected = _items.Where(i => i.SelectedQuantity > 0).ToList();
            if (!selected.Any())
            {
                MessageBox.Show("Hautatu gutxienez produktu bat.", "Atención",
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

                LoadProducts();
                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        private FlowDocument BuildFlowDocumentFromText(string text)
        {
            var doc = new FlowDocument { FontSize = 12 };
            foreach (var line in text.Split(Environment.NewLine))
                doc.Blocks.Add(new Paragraph(new Run(line)));
            return doc;
        }
    }
}
