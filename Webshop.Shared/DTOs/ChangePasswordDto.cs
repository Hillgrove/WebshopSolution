using System.ComponentModel.DataAnnotations;

namespace Webshop.Shared.DTOs
{
    public class ChangePasswordDto
    {
        [Required, MinLength(8), MaxLength(64)]
        public required string OldPassword { get; set; }

        [Required, MinLength(8), MaxLength(64)]
        public required string NewPassword { get; set; }
    }
}
