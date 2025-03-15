using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AlIssam.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Request
{
    public class CreateProductRequest
    {
        // Arabic fields
        [FromForm(Name = "Name_Ar")]
        public string Name_Ar { get; set; }

        [FromForm(Name = "Description_Ar")]
        public string Description_Ar { get; set; }

        // English fields
        [FromForm(Name = "Name_En")]
        public string Name_En { get; set; }

        [FromForm(Name = "Description_En")]
        public string Description_En { get; set; }

        public List<ProductOptions>? Quantities { get; set; }

        [FromForm(Name = "Unit_Of_Measurement_Ar")]
        public string Unit_Of_Measurement_Ar { get; set; }

        [FromForm(Name = "Unit_Of_Measurement_En")]
        public string Unit_Of_Measurement_En { get; set; }

        [FromForm(Name = "Stock")]
        public int Stock { get; set; }

        [JsonIgnore]
        public List<int> Categories_Ids { get; set; }

        [JsonIgnore]
        public List<IFormFile> ProductImages {  get; set; }

        // Cover image
        [FromForm(Name = "Cover_Image")]
        public IFormFile CoverImage { get; set; }



    }

    public class Quantity
    {
        public decimal Quantity_In_Unit { get; set; }
        public decimal Price { get; set; }
        public decimal? Offer { get; set; }
        public bool Default { get; set; }
    }
}