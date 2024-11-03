using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate;

public class CourierEntityTypeConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.ToTable("couriers");

        builder.HasKey(entity => entity.Id);

        builder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .IsRequired();

        builder
            .Property(entity => entity.Name)
            .HasColumnName("name")
            .IsRequired();

        builder
            .HasOne(entity => entity.Transport)
            .WithMany()
            .IsRequired()
            .HasForeignKey("transport_id");

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
        
        builder
            .Property(entity => entity.OrderId)
            .HasColumnName("order_id")
            .IsRequired(false);
            
        builder
            .HasOne<Order>()
            .WithMany()
            .HasForeignKey(entity => entity.OrderId);
        
        builder
            .OwnsOne(entity => entity.OrderLocation, l =>
            {
                l.Property(x => x.X).HasColumnName("order_location_x").IsRequired();
                l.Property(y => y.Y).HasColumnName("order_location_y").IsRequired();
                l.WithOwner();
            });
    }
}