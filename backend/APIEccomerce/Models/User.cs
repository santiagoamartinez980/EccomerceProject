using System.ComponentModel.DataAnnotations;

namespace APIEccomerce.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required, EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = null!;

        [Required]
        public Role Role { get; set; } = Role.User;

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}

