using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities.Config
{
    public class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Name_Ar)
            .IsUnique(); // Unique constraint for Name_Ar

            builder.HasIndex(x => x.Name_En)
                .IsUnique(); // Unique constraint for Name_Ar

        }
    }
}
