using System.ComponentModel.DataAnnotations;

namespace TPV_Gastronomico.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int MesaId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }

        [Required]
        public TipoComida Tipo { get; set; }

        // TEMPORAL: Comentamos Estado hasta que la BD se actualice
        // public string Estado { get; set; } = "Activa";

        public int NumPersonas { get; set; }

        // Navigation properties
        public virtual Mesa Mesa { get; set; }
        public virtual User Usuario { get; set; }
    }
}