using System.ComponentModel.DataAnnotations;

namespace TPV_Gastronomico.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }
        [Required] public string Nombre { get; set; }
        public string Categoria { get; set; }
        [Required] public decimal Precio { get; set; }
        [Required] public int Cantidad { get; set; }
    }
}
