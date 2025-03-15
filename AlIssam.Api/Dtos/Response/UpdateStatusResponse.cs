namespace AlIssam.API.Dtos.Response
{
    public class UpdateStatusResponse
    {
        public string Id { get; set; } // Order ID
        public string Status_En { get; set; } // Status in English
        public string Status_Ar { get; set; } // Status in Arabic
        public DateTime Order_Date { get; set; } // Order date
        public string User_Name { get; set; } // User name
        public decimal Total_Amount { get; set; } // Total amount without discount
        public decimal? Total_Amount_With_Discount { get; set; } // Total amount with discount
    }
}