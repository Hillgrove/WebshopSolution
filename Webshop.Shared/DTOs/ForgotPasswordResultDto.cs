using Webshop.Shared.Enums;

namespace Webshop.Shared.DTOs
{
    public class ForgotPasswordResultDto
    {
        public bool Success { get; set; }
        public ErrorCode? Error { get; set; }
        public string? Message { get; set; }
    }
}
