namespace TaskAi.Api.Models
{
    using System;
    public class Document
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string FileName { get; set; } = null!;
        public string? FileType { get; set; }
        public string StoragePath { get; set; } = null!;
        public string? ExtractedSummary { get; set; }
        public string? ExtractedData { get; set; } // JSONB

        public DateTime CreatedAt { get; set; }
    }
}
