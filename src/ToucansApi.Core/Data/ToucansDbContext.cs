using Microsoft.EntityFrameworkCore;
using ToucansApi.Core.Models;

namespace ToucansApi.Core.Data;

public class ToucansDbContext : DbContext
{
    public ToucansDbContext(DbContextOptions<ToucansDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoList> TodoLists { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TodoListShare> TodoListShares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoList>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValue(DateTime.UtcNow);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Items)
                .WithOne(i => i.List)
                .HasForeignKey(i => i.ListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.SharedWith)
                .WithOne(s => s.TodoList)
                .HasForeignKey(s => s.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.Priority)
                .HasConversion<string>();

            entity.Property(e => e.Status)
                .HasConversion<string>();

            // Relationship with User (AssignedTo)
            entity.HasOne(e => e.AssignedTo)
                .WithMany()
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.List)
                .WithMany(l => l.Items)
                .HasForeignKey(e => e.ListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TodoListShare>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.TodoList)
                .WithMany(l => l.SharedWith)
                .HasForeignKey(e => e.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SharedWithUser)
                .WithMany()
                .HasForeignKey(e => e.SharedWithUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasMany(e => e.OwnedLists)
                .WithOne(l => l.Owner)
                .HasForeignKey(l => l.OwnerId);
        });
    }
}