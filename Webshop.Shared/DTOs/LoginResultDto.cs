using Webshop.Shared.Enums;

namespace Webshop.Shared.DTOs
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public LoginErrorCode? Error { get; set; }
        public required string Message { get; set; }
    }
}
