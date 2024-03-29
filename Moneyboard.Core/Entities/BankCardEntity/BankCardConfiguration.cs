﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Moneyboard.Core.Entities.BankCardEntity
{
    public class BankCardConfiguration : IEntityTypeConfiguration<BankCard>
    {
        public void Configure(EntityTypeBuilder<BankCard> builder)
        {
            builder
                .HasKey(x => x.BankCardId);

            builder
                .Property(x => x.CardNumber)
                .IsRequired()
                .HasMaxLength(16);

            builder
                .Property(x => x.Money)
                .IsRequired();

            builder
                .Property(x => x.CardVerificationValue)
                .HasMaxLength(3)
                .IsRequired();

            builder
                .Property(x => x.ExpirationDate)
                .IsRequired()
                .HasColumnType("date");



            builder
                .HasMany(x => x.Projects)
                .WithOne(x => x.BankCard)
                .HasForeignKey(x => x.BankCardId);


        }
    }
}
