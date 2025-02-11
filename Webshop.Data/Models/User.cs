using System.ComponentModel.DataAnnotations;

namespace Webshop.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, EmailAddress]
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiration { get; set; }
    }
}
