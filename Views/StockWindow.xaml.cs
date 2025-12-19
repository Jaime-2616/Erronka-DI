using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Services;

namespace TPV_Gastronomico.Views
{
    public partial class StockWindow : Window
    {
        private readonly StockService _stockService = new StockService();
        private Producto _selectedProducto;

        public StockWindow()
        {
            // Euskera: Stock kudeaketa leihoa; produktu zerrenda kargatu
            InitializeComponent();
            LoadAll();
        }

        private void LoadAll()
        {
            try
            {
                var list = _stockService.GetAll().ToList();
                ProductosGrid.ItemsSource = list;
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refrescar_Click(object sender, RoutedEventArgs e) => LoadAll();

        private void Buscar_Click(object sender, RoutedEventArgs e)
        {
            // Euskera: Produktuak bilatu izen edo kategoriaren arabera
            var term = SearchBox.Text;
            var list = _stockService.Search(term).ToList();
            ProductosGrid.ItemsSource = list;
            ClearForm();
        }

        private void StockBajo_Click(object sender, RoutedEventArgs e)
        {
            // Euskera: Stock baxuko produktuak erakutsi
            var list = _stockService.GetLowStock(5).ToList();
            ProductosGrid.ItemsSource = list;
            ClearForm();
        }

        private void ProductosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Euskera: Hautatutako produktuaren datuak formularioan erakutsi
            _selectedProducto = ProductosGrid.SelectedItem as Producto;
            if (_selectedProducto != null)
            {
                NombreBox.Text = _selectedProducto.Nombre;
                CategoriaBox.Text = _selectedProducto.Categoria;
                PrecioBox.Text = _selectedProducto.Precio.ToString("G");
                CantidadBox.Text = _selectedProducto.Cantidad.ToString();
            }
        }

        private void ClearForm()
        {
            _selectedProducto = null;
            NombreBox.Text = "";
            CategoriaBox.Text = "";
            PrecioBox.Text = "";
            CantidadBox.Text = "";
            ProductosGrid.UnselectAll();
        }

        private void Anadir_Click(object sender, RoutedEventArgs e)
        {
            // Euskera: Produktu berria gehitu datu-basera
            try
            {
                var nuevo = new Producto
                {
                    Nombre = NombreBox.Text.Trim(),
                    Categoria = CategoriaBox.Text.Trim(),
                    Precio = decimal.TryParse(PrecioBox.Text.Trim(), out var p) ? p : 0m,
                    Cantidad = int.TryParse(CantidadBox.Text.Trim(), out var c) ? c : 0
                };

                _stockService.Add(nuevo);
                LoadAll();
                MessageBox.Show("Producto añadido.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo añadir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            // Euskera: Hautatutako produktua eguneratu
            if (_selectedProducto == null)
            {
                MessageBox.Show("Selecciona un producto para actualizar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _selectedProducto.Nombre = NombreBox.Text.Trim();
                _selectedProducto.Categoria = CategoriaBox.Text.Trim();
                _selectedProducto.Precio = decimal.TryParse(PrecioBox.Text.Trim(), out var p) ? p : 0m;
                _selectedProducto.Cantidad = int.TryParse(CantidadBox.Text.Trim(), out var c) ? c : 0;

                _stockService.Update(_selectedProducto);
                LoadAll();
                MessageBox.Show("Producto actualizado.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo actualizar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            // Euskera: Hautatutako produktuaren ezabapena baieztatu eta egin
            if (_selectedProducto == null)
            {
                MessageBox.Show("Selecciona un producto para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"¿Eliminar '{_selectedProducto.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _stockService.Delete(_selectedProducto.Id);
                LoadAll();
                MessageBox.Show("Producto eliminado.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
