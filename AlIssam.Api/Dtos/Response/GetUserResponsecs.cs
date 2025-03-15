namespace AlIssam.API.Dtos.Response
{
    public class GetUserResponse
    {
        public string Id { get; set; }
        public string User_Name { get; set; }
        public string Address { get; set; }
        public string Phone_Number { get; set; }
        public string Email { get; set; }
        public bool Is_Confirmed { get; set; }
        public List<GetOrderResponse> Orders { get; set; }
    }

    public class GetOrderResponse
    {
        public string Id { get; set; }
        public DateTime Order_Date { get; set; }
        public string Status_Ar { get; set; }
        public string Status_En { get; set; }
        public List<GetOrderDetailResponse> Order_Details { get; set; }
    }

    public class GetOrderDetailResponse
    {
        public int Id { get; set; }
        public int Product_Id { get; set; }
        public decimal Unit_Price { get; set; }
        public int Quantity_Of_Unit { get; set; }
        public decimal? Offer { get; set; }
    }
}
