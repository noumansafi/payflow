using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.ReferenceNumber)
            .IsUnique();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Fee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => new { x.SenderWalletId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.ReceiverWalletId, x.CreatedAtUtc });
    }
}
