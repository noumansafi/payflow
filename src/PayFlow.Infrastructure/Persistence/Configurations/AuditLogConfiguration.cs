using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasMaxLength(4000);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(64);

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => new { x.ActorUserId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.Action, x.CreatedAtUtc });
    }
}
