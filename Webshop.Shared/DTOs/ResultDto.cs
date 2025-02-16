using Webshop.Shared.Enums;

namespace Webshop.Shared.DTOs
{
    public class ResultDto
    {
        public bool Success { get; set; }
        public ErrorCode? Error { get; set; }
        public string? Message { get; set; }
    }
}
