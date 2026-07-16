using Lofasi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lofasi.Infrastructure.Persistence.Configurations;

public sealed class AccountTransactionConfiguration : IEntityTypeConfiguration<AccountTransaction>
{
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.BankAccountId).IsRequired();
        builder.Property(transaction => transaction.Type).IsRequired();
        builder.Property(transaction => transaction.AmountInCents).IsRequired();
        builder.Property(transaction => transaction.BalanceAfterTransactionInCents).IsRequired();
        builder.Property(transaction => transaction.OccurredAtUtc).IsRequired();

        builder.HasIndex(transaction => new { transaction.BankAccountId, transaction.OccurredAtUtc });
    }
}
