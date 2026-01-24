namespace TaskAi.Api.Models
{
    using System;
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
