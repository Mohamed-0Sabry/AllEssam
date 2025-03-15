using System;
using System.Collections.Generic;

namespace AlIssam.DataAccessLayer.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Slug_Ar { get; set; }
        public string Slug_En { get; set; }

        // Arabic fields
        public string Name_Ar { get; set; } // Arabic name
        public string Description_Ar { get; set; } // Arabic description

        // English fields
        public string Name_En { get; set; } // English name
        public string Description_En { get; set; } // English description

        // Common fields
        public ICollection<ProductOptions> Quantity { get; set; }

        // Relationships
        public ICollection<ProductImage> ProductsImages { get; set; }

        public bool IsEnded { get; set; }
        public ICollection<CategoriesProducts> CategoriesProducts { get; set; }
        // public ICollection<OrderDetails> OrderDetails { get; set; }

        // New fields for Unit of Measurement and Stock
        public string Unit_Of_Measurement_Ar { get; set; } // الوحدة القياسية (Arabic)
        public string Unit_Of_Measurement_En { get; set; } // Unit of Measurement (English)
        public int Stock { get; set; } // الكمية المتبقية (Remaining Quantity or Stock)
    }
}