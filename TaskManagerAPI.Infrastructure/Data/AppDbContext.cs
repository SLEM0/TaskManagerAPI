using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Domain.Entities;

namespace TaskManagerAPI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<TaskList> TaskLists { get; set; }
    public DbSet<Domain.Entities.Task> Tasks { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<BoardUser> BoardUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка связи многие-ко-многим для Task и Label
        modelBuilder.Entity<Domain.Entities.Task>()
            .HasMany(t => t.Labels)
            .WithMany() // ← Без обратной навигации!
            .UsingEntity(j => j.ToTable("TaskLabels"));

        // Уникальный составной ключ (чтобы пользователь не мог быть добавлен в доску дважды)
        modelBuilder.Entity<BoardUser>()
            .HasIndex(bu => new { bu.BoardId, bu.UserId })
            .IsUnique();
    }
}