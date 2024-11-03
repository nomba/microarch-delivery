using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate;

internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(entity => entity.Id);

        builder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .IsRequired();

        builder
            .Property(entity => entity.CourierId)
            .HasColumnName("courier_id")
            .IsRequired(false);
        
        // Must set relationship (bellow) between Order and Courier even Order stores only Courier.ID, not whole object
        // It provides consistency in database.
        
        builder
            .HasOne<Courier>()
            .WithMany()
            .HasForeignKey(entity => entity.CourierId);

        builder
            .HasOne(entity => entity.Status)
            .WithMany()
            .IsRequired()
            .HasForeignKey("status_id");

        builder
            .OwnsOne(entity => entity.Location, l =>
            {
                l.Property(x => x.X).HasColumnName("location_x").IsRequired();
                l.Property(y => y.Y).HasColumnName("location_y").IsRequired();
                l.WithOwner();
            });
    }
}