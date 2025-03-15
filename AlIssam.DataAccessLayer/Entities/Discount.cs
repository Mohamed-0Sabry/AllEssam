using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities
{
    public class Discount
    {
        [JsonPropertyName("id")]
        public int DiscountId { get; set; }
        public string Code { get; set; }
        public DateTime StarDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Percent { get; set; }
        public bool Status { get; set; }    
        public ICollection<Order>? Orders{ get; set; }
    }
}
