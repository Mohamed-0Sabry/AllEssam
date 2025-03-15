using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities
{
    public class ContactInfo
    {
        [JsonIgnore]
        public int Id { get; set; }

        // Phone Number
        public string Phone_Number { get; set; } 

        // Location in Arabic and English
        public string Location_Ar { get; set; }
        public string Location_En { get; set; }

        // Social Media Links
        public string Instagram { get; set; } 
        public string WhatsApp { get; set; } 
        public string TikTok { get; set; } 
    }
}
