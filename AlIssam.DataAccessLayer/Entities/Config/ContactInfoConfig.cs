using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities.Config
{
    public class ContactInfoConfig : IEntityTypeConfiguration<ContactInfo>
    {
        public void Configure(EntityTypeBuilder<ContactInfo> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasData( new ContactInfo {
                Id = 1,
                Phone_Number = "+96525476832",
                Instagram = "instrgam.com",
                TikTok = "tiktok.com"  ,
                Location_Ar = "الكويت",
                Location_En = "Kuwait",
                WhatsApp = "whatsApp"
            });
        }
    }
}
