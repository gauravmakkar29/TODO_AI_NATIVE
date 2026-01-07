using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Todo> Todos { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TodoCategory> TodoCategories { get; set; }
    public DbSet<TodoTag> TodoTags { get; set; }
    public DbSet<FilterPreset> FilterPresets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Performance indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Performance indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsCompleted });
            entity.HasIndex(e => new { e.UserId, e.Priority });
            entity.HasIndex(e => new { e.UserId, e.DueDate });
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7); // Hex color code
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7); // Hex color code
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure many-to-many relationship: Todo <-> Category
        modelBuilder.Entity<TodoCategory>(entity =>
        {
            entity.HasKey(e => new { e.TodoId, e.CategoryId });
            entity.HasOne(e => e.Todo)
                  .WithMany(t => t.TodoCategories)
                  .HasForeignKey(e => e.TodoId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.TodoCategories)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Performance indexes
            entity.HasIndex(e => e.TodoId);
            entity.HasIndex(e => e.CategoryId);
        });

        // Configure many-to-many relationship: Todo <-> Tag
        modelBuilder.Entity<TodoTag>(entity =>
        {
            entity.HasKey(e => new { e.TodoId, e.TagId });
            entity.HasOne(e => e.Todo)
                  .WithMany(t => t.TodoTags)
                  .HasForeignKey(e => e.TodoId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.TodoTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Performance indexes
            entity.HasIndex(e => e.TodoId);
            entity.HasIndex(e => e.TagId);
        });

        modelBuilder.Entity<FilterPreset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SearchQuery).HasMaxLength(500);
            entity.Property(e => e.CategoryIds).HasMaxLength(500);
            entity.Property(e => e.TagIds).HasMaxLength(500);
            entity.Property(e => e.SortBy).HasMaxLength(50);
            entity.Property(e => e.SortOrder).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Performance index
            entity.HasIndex(e => e.UserId);
        });
    }
}

