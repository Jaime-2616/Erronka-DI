using System.Windows;
using TPV_Gastronomico.Controllers;
using TPV_Gastronomico.Models;

namespace TPV_Gastronomico.Views
{
    public partial class ReservationDialogWindow : Window
    {
        private readonly ReservationController _controller;

        public ReservationDialogWindow(User usuario)
        {
            InitializeComponent();
            _controller = new ReservationController(ReservationControl, usuario);
            _controller.ReservaCreated += Controller_ReservaCreated;
            _controller.Cancelled += Controller_Cancelled;
        }

        private void Controller_ReservaCreated(object sender, Reserva e)
        {
            // Close dialog with success
            DialogResult = true;
            Close();
        }

        private void Controller_Cancelled(object sender, System.EventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}