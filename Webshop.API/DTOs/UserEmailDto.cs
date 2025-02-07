using System.ComponentModel.DataAnnotations;

namespace Webshop.API.DTOs
{
    public class UserEmailDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }
    }
}
