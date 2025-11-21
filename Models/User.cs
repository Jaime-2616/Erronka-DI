using System.ComponentModel.DataAnnotations;

namespace TPV_Gastronomico.Models
{
    public enum Role { Admin, Usuario }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public Role Rol { get; set; }
    }
}
