using Microsoft.AspNetCore.Mvc;

namespace AlIssam.API.Dtos.Request
{
    public class UpdateCategoryDto
    {
        [FromForm(Name = "Name_En")]
        public string Name_En { get; set; }

        [FromForm(Name = "Name_Ar")]
        public string Name_Ar { get; set; }

        [FromForm(Name = "Image")]
        public IFormFile? Image { get; set; } // For image upload (optional during update)
    }
}
