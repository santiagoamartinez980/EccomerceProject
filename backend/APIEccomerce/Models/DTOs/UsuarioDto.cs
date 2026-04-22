
using System.ComponentModel.DataAnnotations;

namespace APIEccomerce.Models.DTOs
{
    public class UsuarioDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Apellidos { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Correo inválido")] 
        public string Correo { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        public string Clave { get; set; } = null!;
    }
}
