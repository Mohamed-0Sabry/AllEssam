using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities
{
    public class OrderReceiverDetails
    {
        public int OrderReceiverDetailsId { get; set; } 
        public string Location { get; set; }
        public string Recevier { get; set; }
        public string Phone { get; set; }
        public string OrderId { get; set; }
        public Order Order { get; set; }
    }
}
