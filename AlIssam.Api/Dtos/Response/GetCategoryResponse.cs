namespace AlIssam.API.Dtos.Response
{

    public class GetCategoryResponse
    {
        public int Id { get; set; }
        public string Name_Ar { get; set; }
        public string Name_En { get; set; }
        public string Slug_Ar { get; set; }
        public string Slug_En { get; set; }
        public string Color { get; set; }
        public int Number_Of_Products { get; set; }
        public string  Image { get; set; } // Example: List of image paths
    }
}
