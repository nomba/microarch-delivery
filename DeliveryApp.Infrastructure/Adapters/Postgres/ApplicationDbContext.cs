using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox;
using Microsoft.EntityFrameworkCore;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Courier> Couriers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply Configuration
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderStatusEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CourierEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CourierStatusEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TransportEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxEntityTypeConfiguration());

        // Seed
        modelBuilder.Entity<OrderStatus>(b =>
        {
            var allStatuses = OrderStatus.List();
            b.HasData(allStatuses.Select(c => new {c.Id, c.Name}));
        });

        modelBuilder.Entity<CourierStatus>(b =>
        {
            var allStatuses = CourierStatus.List();
            b.HasData(allStatuses.Select(c => new {c.Id, c.Name}));
        });
        
        // Seed transport with owned entity speed. Learn more:
        // https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding#model-managed-data
        
        var allTransports = Transport.List();
        
        modelBuilder.Entity<Transport>(b =>
        {
            b.HasData(allTransports.Select(c => new {c.Id, c.Name}));
        });

        modelBuilder.Entity<Transport>().OwnsOne(p => p.Speed).HasData(
            allTransports.Select(t => new {TransportId = t.Id, t.Speed.Value}));
    }

    public async Task<bool> SaveEntities(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
        return true;
    }
}