using System.ComponentModel.DataAnnotations;

namespace Webshop.API.DTOs
{
    public class UserRegistrationDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(8)]
        public required string Password { get; set; }
    }
}
