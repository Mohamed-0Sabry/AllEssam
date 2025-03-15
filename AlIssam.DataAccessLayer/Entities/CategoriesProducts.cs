using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities
{
    public class CategoriesProducts
    {
        public int Id { get; set; } 
        public int ProductId { get; set; } // Foreign key to Product
        public int CategoryId { get; set; } // Foreign key to Category

        // Navigation properties
        public Product Product { get; set; }
        public Category Category { get; set; }
    }
}
