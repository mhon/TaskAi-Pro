using System;
using System.ComponentModel.DataAnnotations.Schema; // Add this using

namespace TaskAi.Api.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        [Column("due_date")] // Maps C# DueDate to Postgres due_date
        public DateTime? DueDate { get; set; }

        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "To Do";
        public string? Category { get; set; }

        [Column("user_id")] // Maps C# UserId to Postgres user_id
        public Guid UserId { get; set; }

        public User? User { get; set; }

        [Column("created_at")] // Maps C# CreatedAt to Postgres created_at
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")] // Maps C# UpdatedAt to Postgres updated_at
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}