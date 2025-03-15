namespace AlIssam.API.Dtos.Request
{
    public class CreateOrderDetailRequest
    {
        public int Product_Id { get; set; } // ID of the product
        public int Product_Option_Id { get; set; }
        public int Quantity { get; set; } // Quantity of the product
    }
}
