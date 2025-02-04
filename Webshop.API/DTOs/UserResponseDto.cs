using System.ComponentModel.DataAnnotations;

namespace Webshop.API.DTOs
{
    public class UserResponseDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }
    }
}
