using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Configurations;

public sealed class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.ToTable("Beneficiaries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(150);

        builder.HasIndex(x => new { x.OwnerUserId, x.BeneficiaryUserId })
            .IsUnique();

        builder.HasOne(x => x.BeneficiaryUser)
            .WithMany()
            .HasForeignKey(x => x.BeneficiaryUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
