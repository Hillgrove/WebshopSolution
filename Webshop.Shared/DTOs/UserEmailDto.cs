using System.ComponentModel.DataAnnotations;

namespace Webshop.Shared.DTOs
{
    public class UserEmailDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        // FingerPrintJS string
        public string? VisitorId { get; set; }
    }
}
