using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities.Config
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);

            // Configure the one-to-one relationship with Payment
            builder.HasOne(x => x.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if needed

            builder.Property(o => o.Payment_Method).HasDefaultValue("cash");
            // Configure the one-to-one relationship with Payment
            builder.HasOne(o => o.ReceiverDetails)
            .WithOne(ord => ord.Order)
            .HasForeignKey<OrderReceiverDetails>(ord => ord.OrderId)
            //.HasForeignKey<Order>(ord => ord.OrderReceiverDetailsId)
            .OnDelete(DeleteBehavior.Cascade);

            // Configure the one-to-many relationship with User
            builder.HasOne<User>(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the one-to-many relationship with OrderDetails
            builder.HasMany(x => x.OrdersDetails)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict or ClientSetNull
        }
    }
}
