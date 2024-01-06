using AspAuthentication.Auth.Enums;
using System.ComponentModel.DataAnnotations;

namespace AspAuthentication.Data.Models
{
    public class RegistrationRequest
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public Role Role { get; set; }  // Defaults to User role
    }
}
