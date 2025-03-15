using Microsoft.AspNetCore.Mvc;
using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.API.Dtos.Request
{
    public class UpdateProductRequest
    {
        [FromForm(Name = "Name_Ar")]
        public string Name_Ar { get; set; }

        [FromForm(Name = "Description_Ar")]
        public string Description_Ar { get; set; }

        [FromForm(Name = "Name_En")]
        public string Name_En { get; set; }

        [FromForm(Name = "Description_En")]
        public string DescriptionEn { get; set; }

        [FromForm(Name = "CoverImage")]
        public IFormFile? CoverImage { get; set; } // Handle cover image as a file

        public List<ProductOptions>? Quantities { get; set; }

        [FromForm(Name = "Unit_Of_Measurement_Ar")]
        public string Unit_Of_Measurement_Ar { get; set; }

        [FromForm(Name = "Unit_Of_Measurement_En")]
        public string Unit_Of_Measurement_En { get; set; }

        [FromForm(Name = "Stock")]
        public int Stock { get; set; }

        public List<int> Categories_Ids { get; set; }
        public List<IFormFile> Product_New_Images { get; set; }
        public List<string> Product_Images_Paths { get; set; }


        // Categories will be handled dynamically from the form data
    }

}
