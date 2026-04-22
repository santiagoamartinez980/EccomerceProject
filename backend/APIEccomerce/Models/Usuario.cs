using System.ComponentModel.DataAnnotations;

namespace APIEccomerce.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Apellidos { get; set; } = null!;

        [Required, EmailAddress]
        [MaxLength(150)]
        public string Correo { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Clave { get; set; } = null!; 

        [Required]
        public Rol Rol { get; set; } = Rol.Usuario;
    }
}
