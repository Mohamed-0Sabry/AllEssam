using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AlIssam.DataAccessLayer.Entities;

public class ProductImageConfig : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasIndex(x => x.Id);

        // Configure the relationship with Product
        builder.HasOne(x => x.Product)
               .WithMany(p => p.ProductsImages) // Specify the navigation property in Product
               .HasForeignKey(x => x.ProductId) // Specify the foreign key
               .OnDelete(DeleteBehavior.Cascade);
    }
}