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
    public DbSet<Member> Members { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Task>()
            .HasMany(t => t.Labels)
            .WithMany()
            .UsingEntity(j => j.ToTable("TaskLabels"));

        modelBuilder.Entity<Domain.Entities.Task>()
            .HasMany(t => t.Members)
            .WithMany()
            .UsingEntity(j => j.ToTable("TaskAssignees"));

        modelBuilder.Entity<Comment>()
        .HasOne(c => c.Task)
        .WithMany(t => t.Comments)
        .HasForeignKey(c => c.TaskId);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId);

        modelBuilder.Entity<Member>()
            .HasIndex(bu => new { bu.BoardId, bu.UserId })
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
        .HasOne(rt => rt.User)
        .WithMany()
        .HasForeignKey(rt => rt.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}