using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Request
{

    public class GetProductRequest
    {
        public string Image { get; set; } // URL or path to the image

        [FromForm(Name = "name_ar")]
        public string NameAr { get; set; }

        [FromForm(Name = "description_ar")]
        public string DescriptionAr { get; set; }

        // English fields
        [FromForm(Name = "name_en")]
        public string NameEn { get; set; }

        [FromForm(Name = "description_en")]
        public string DescriptionEn { get; set; }

        // Quantities
        [FromForm(Name = "quantities")]
        [JsonConverter(typeof(QuantitiesConverter))]
        public List<Quantity> Quantities { get; set; }
        public decimal? Offer { get; set; }
        public int CategoryId { get; set; }
    }
}
