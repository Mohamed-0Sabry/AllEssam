using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities.Config
{
    public class DiscountConfig : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.HasKey(x => x.DiscountId);

            builder.HasMany(x => x.Orders)
                   .WithOne()
                   .HasForeignKey(x => x.DiscountId)
                   .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
