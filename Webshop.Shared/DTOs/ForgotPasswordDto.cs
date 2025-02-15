using System.ComponentModel.DataAnnotations;

namespace Webshop.Shared.DTOs
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        // FingerPrintJS string
        public string? VisitorId { get; set; }
    }
}
