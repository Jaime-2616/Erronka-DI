using System;
using System.Windows;
using System.Windows.Controls;

namespace TPV_Gastronomico.Views
{
    public partial class ProductItemControl : UserControl
    {
        // Simple CLR event the parent view can handle
        public event EventHandler SelectQuantityRequested;

        public ProductItemControl()
        {
            InitializeComponent();
        }

        private void SelectQuantity_Click(object sender, RoutedEventArgs e)
        {
            SelectQuantityRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}