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
    public class CategoriesProductsConfig : IEntityTypeConfiguration<CategoriesProducts>
    {
        public void Configure(EntityTypeBuilder<CategoriesProducts> builder)
        {
            builder.HasKey(x => x.Id);

            builder
            .HasOne(cp => cp.Category)
            .WithMany(c => c.CategoriesProducts)
            .HasForeignKey(cp => cp.CategoryId);

            builder
            .HasOne(cp => cp.Product)
            .WithMany(p => p.CategoriesProducts)
            .HasForeignKey(cp => cp.ProductId);

            //builder
            //.HasIndex(cp => cp.ProductId)
            //.HasDatabaseName("IX_CategoriesProducts_ProductId");

            //// Add index on CategoryId
            //builder
            //    .HasIndex(cp => cp.CategoryId)
            //    .HasDatabaseName("IX_CategoriesProducts_CategoryId");

        }
    }
}
