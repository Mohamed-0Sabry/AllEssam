using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities.Config
{
    public class OrderDetailsConfig : IEntityTypeConfiguration<OrderDetails>
    {
        public void Configure(EntityTypeBuilder<OrderDetails> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Order)
                .WithMany(x => x.OrdersDetails)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductOptions)
               .WithMany()
               .HasForeignKey(x => x.ProductOptionsId)
               .OnDelete(DeleteBehavior.Restrict);



            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
