namespace AlIssam.API.Dtos.Response
{
    public class GetProductBySlugResponse
    {
        public GetProductResponse product { get; set; }
        public List<GetProductResponse> similarProducts { get; set; }
    }
}
