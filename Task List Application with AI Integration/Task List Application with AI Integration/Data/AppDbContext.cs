using Microsoft.EntityFrameworkCore;
using TaskAi.Api.Models;

namespace TaskAi.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("tasks");
                entity.Property(t => t.Id).HasColumnName("id");
                entity.Property(t => t.Title).HasColumnName("title");
                entity.Property(t => t.Description).HasColumnName("description");
                entity.Property(t => t.DueDate).HasColumnName("due_date");
                entity.Property(t => t.Priority).HasColumnName("priority");
                entity.Property(t => t.Status).HasColumnName("status");
                entity.Property(t => t.Category).HasColumnName("category");
                entity.Property(t => t.UserId).HasColumnName("user_id");
            });


            builder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.AuthProvider).HasColumnName("auth_provider");
                entity.Property(e => e.ProviderUserId).HasColumnName("provider_user_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            builder.Entity<User>()
                .HasMany(u => u.Tasks)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}