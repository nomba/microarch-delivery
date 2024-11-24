using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox;

internal class OutboxEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder
            .ToTable("outbox");

        builder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder
            .Property(entity => entity.Type)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("type")
            .IsRequired();

        builder
            .Property(entity => entity.Content)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("content")
            .IsRequired();

        builder
            .Property(entity => entity.OccuredAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("occured_at_utc")
            .IsRequired();

        builder
            .Property(entity => entity.HandledAtUtc)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("handled_at_utc")
            .IsRequired(false);
    }
}