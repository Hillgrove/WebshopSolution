using Webshop.Data.Models;

namespace Webshop.Shared.DTOs
{
    public class RegistrationResultDto
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public User? User { get; set; }
    }
}
