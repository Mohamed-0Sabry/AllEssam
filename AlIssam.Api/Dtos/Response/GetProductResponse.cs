using Microsoft.AspNetCore.Mvc;
using AlIssam.API.Dtos.Request;
using AlIssam.DataAccessLayer.Entities;
using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Response
{
    public class GetProductResponse
    {
        public int Id { get; set; }

        //[JsonPropertyName("Quantitiy_In_Unit")]
        public int Stock { get; set; }
        public string Name_Ar { get; set; }
        public string Description_En { get; set; }
        public string Name_En { get; set; }
        public string Description_Ar { get; set; }
        public string Slug_Ar { get; set; }
        public string Slug_En { get; set; }
        public string Cover_Image { get; set; }
        public string Unit_Of_Measurement_Ar { get; set; }
        public string Unit_Of_Measurement_En { get; set; }


        public List<string> ImagesPath { get; set; }
        public List<CategoryDto> Categories { get; set; } // Updated to list of categories
        public List<GetProductOptionsResponse> Quantities { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name_Ar { get; set; }
        public string Name_En { get; set; }
    }
}
