using laboratorul4.Features.Orders;
using Microsoft.EntityFrameworkCore;

namespace laboratorul4.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) {}

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.ISBN).IsUnique();
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });
    }
}