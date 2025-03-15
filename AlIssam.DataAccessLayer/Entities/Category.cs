using System;
using System.Collections.Generic;

namespace AlIssam.DataAccessLayer.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name_En { get; set; } 
        public string Slug_En { get; set; } 
        public string Slug_Ar { get; set; }
        public string Name_Ar { get; set; } // Arabic name
        public string ImagePath { get; set; } // Path to the category image
        public ICollection<CategoriesProducts> CategoriesProducts { get; set; }

    }
}