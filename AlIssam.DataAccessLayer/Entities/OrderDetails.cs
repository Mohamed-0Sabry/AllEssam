using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public Order Order { get; set;  }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int? ProductOptionsId { get; set; }
        public ProductOptions? ProductOptions { get; set; }
        public decimal Unit_Price { get; set; }
        public decimal? Offer { get ; set; }
        public int Quantity_Of_Unit { get; set; }

    }
}
