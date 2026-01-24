namespace TaskAi.Api.Models
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string AuthProvider { get; set; } = "local";
        public string? ProviderUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class RegisterDto
    {
        [Required]
        public required string Username { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(5)]
        public required string Password { get; set; }
    }
}
