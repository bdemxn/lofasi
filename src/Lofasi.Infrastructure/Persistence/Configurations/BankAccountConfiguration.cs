using Lofasi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lofasi.Infrastructure.Persistence.Configurations;

public sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).ValueGeneratedNever();

        builder.Property(account => account.CustomerId).IsRequired();

        builder.Property(account => account.AccountNumber)
            .IsRequired()
            .HasMaxLength(17);

        builder.HasIndex(account => account.AccountNumber).IsUnique();

        builder.Property(account => account.BalanceInCents).IsRequired();
        builder.Property(account => account.CreatedAtUtc).IsRequired();

        builder.HasMany(account => account.Transactions)
            .WithOne(transaction => transaction.BankAccount)
            .HasForeignKey(transaction => transaction.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(account => account.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
