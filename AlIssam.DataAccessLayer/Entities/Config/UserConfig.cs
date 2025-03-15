using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities.Config
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.Role).HasDefaultValue("customer");


            builder.HasIndex(x => x.UserName)
                    .IsUnique(); // Unique constraint for Name_Ar
        }
    }
}
