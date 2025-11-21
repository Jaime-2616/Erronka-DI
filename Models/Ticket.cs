using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPV_Gastronomico.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public User Usuario { get; set; }

        [Required]
        public decimal Total { get; set; }

        // Lista de productos asociados al ticket
        public List<TicketDetalle> Detalles { get; set; } = new List<TicketDetalle>();
    }
}
