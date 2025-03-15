namespace AlIssam.API.Dtos.Response
{
    public class OrderSummaryResponse
    {
        public int Id { get; set; }
        public string User_Name { get; set; }
        public string Status_En { get; set; }
        public string Status_Ar { get; set; }
        public DateTime Order_Date { get; set; }
        public decimal Total_Amount { get; set; }
    }
}
