using System;
using System.Windows;

namespace TPV_Gastronomico.Views
{
    public partial class QuantityDialog : Window
    {
        private readonly int _max;
        public int Quantity { get; private set; }

        public QuantityDialog(int maxAvailable, int current = 0)
        {
            InitializeComponent();
            _max = Math.Max(0, maxAvailable);
            Quantity = Math.Min(Math.Max(0, current), _max);
            QuantityBox.Text = Quantity.ToString();
            PromptText.Text = $"Disponible: {_max}";
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if (Quantity < _max) Quantity++;
            QuantityBox.Text = Quantity.ToString();
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            if (Quantity > 0) Quantity--;
            QuantityBox.Text = Quantity.ToString();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityBox.Text, out var q))
            {
                MessageBox.Show("Introduce un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (q < 0) q = 0;
            if (q > _max) q = _max;
            Quantity = q;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}