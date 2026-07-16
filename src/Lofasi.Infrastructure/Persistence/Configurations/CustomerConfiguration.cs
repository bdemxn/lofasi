using Lofasi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lofasi.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.UserId).IsRequired();
        builder.HasIndex(customer => customer.UserId).IsUnique();

        builder.Property(customer => customer.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(customer => customer.DateOfBirth).IsRequired();
        builder.Property(customer => customer.Gender).IsRequired();
        builder.Property(customer => customer.MonthlyIncomeInCents).IsRequired();
        builder.Property(customer => customer.CreatedAtUtc).IsRequired();

        builder.HasMany(customer => customer.Accounts)
            .WithOne(account => account.Customer)
            .HasForeignKey(account => account.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(customer => customer.Accounts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
