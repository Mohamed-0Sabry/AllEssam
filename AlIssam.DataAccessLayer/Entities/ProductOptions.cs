using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities
{
    public class ProductOptions
    {
        public int Id { get; set; }
        public float Quantity_In_Unit { get; set; } 
        public decimal Price { get; set; }
        public decimal? Offer { get; set; }
        public bool Default { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public ICollection<OrderDetails> OrderDetailsList { get; set; }
    }
}
