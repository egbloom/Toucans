using Microsoft.EntityFrameworkCore;
using ToucansApi.Core.Models;

namespace ToucansApi.Core.Data;

public class ToucansDbContext : DbContext
{
    public ToucansDbContext(DbContextOptions<ToucansDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TodoList> TodoLists { get; set; } = null!;
    public DbSet<TodoItem> TodoItems { get; set; } = null!;
    public DbSet<TodoListShare> TodoListShares { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.ToTable("TodoLists");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedLists)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("TodoItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(e => e.TodoList)
                .WithMany(l => l.Items)
                .HasForeignKey(e => e.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AssignedTo)
                .WithMany()
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TodoListShare>(entity =>
        {
            entity.ToTable("TodoListShares");
            entity.HasKey(e => new { e.TodoListId, e.SharedWithUserId });

            entity.HasOne(e => e.TodoList)
                .WithMany(l => l.Shares)
                .HasForeignKey(e => e.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SharedWithUser)
                .WithMany()
                .HasForeignKey(e => e.SharedWithUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoListEvent>(entity =>
        {
            entity.ToTable("TodoListEvents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EventData).HasMaxLength(4000).IsRequired();

            entity.HasOne(e => e.TodoList)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}