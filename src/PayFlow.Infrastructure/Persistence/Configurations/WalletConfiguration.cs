using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Configurations;

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.Balance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasMany(x => x.SentTransactions)
            .WithOne(x => x.SenderWallet)
            .HasForeignKey(x => x.SenderWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ReceivedTransactions)
            .WithOne(x => x.ReceiverWallet)
            .HasForeignKey(x => x.ReceiverWalletId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
