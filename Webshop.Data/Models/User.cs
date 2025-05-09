﻿namespace Webshop.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public string Role { get; set; } = "Customer";
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiration { get; set; }
    }
}
