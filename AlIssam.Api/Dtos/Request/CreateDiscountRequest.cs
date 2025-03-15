namespace AlIssam.API.Dtos.Request
{
    public class CreateDiscountRequest
    {
        public string Code { get; set; } // Discount code
        public DateTime Start_Date { get; set; } // Start date of the discount
        public DateTime End_Date { get; set; } // End date of the discount
        public int Percent { get; set; } // Discount percentage
    }
}
