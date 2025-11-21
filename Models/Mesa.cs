using System.ComponentModel.DataAnnotations;

namespace TPV_Gastronomico.Models
{
    public class Mesa
    {
        [Key]
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }
    }
}
