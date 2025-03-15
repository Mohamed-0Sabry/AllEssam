using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities.Config
{
    public class ProductsConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);
            // Configure the relationship with Category
            builder.Property(x => x.IsEnded).HasDefaultValue(false);

            builder.HasIndex(x => x.Name_Ar)
                .IsUnique(); // Unique constraint for Name_Ar

            builder.HasIndex(x => x.Name_En)
                .IsUnique(); // Unique constraint for Name_En

            builder.HasMany(x => x.Quantity)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
