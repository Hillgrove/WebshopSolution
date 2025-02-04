using System.ComponentModel.DataAnnotations;

namespace Webshop.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, EmailAddress]
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // TODO: Add validation methods? should they be in a validation lib instead?
    }
}
