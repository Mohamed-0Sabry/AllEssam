using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.API.Dtos.Response
{
    public class GetOrderDataResponse
    {
        public string Id { get; set; } // Order ID
        public string User_Name { get; set; } // User name
        public string Email { get; set; }
        public string User_PhoneNumber { get; set; } // User phone number
        public string Status_En { get; set; } // Order status in English
        public string Payment_Method { get; set; } // Order status in English
        public string Receiver_Number { get; set; }
        public string Status_Ar { get; set; } // Order status in Arabic
        public bool IsPaid { get; set; } // Indicates if the order is paid
        public DateTime Order_Date { get; set; } // Order date
        public decimal Total_Amount { get; set; } // Total amount for the order
        public List<OrderDetailsResponse> OrderDetails { get; set; } // List of order details
    }

    public class OrderDetailsResponse
    {
        public string Product_Name_Ar { get; set; }
        public string Product_Name_En { get; set; }

        public string Name_Ar { get; set; }
        public string Name_En { get; set; }
        public int Product_Option_Id { get; set; }  
        public int ProductId { get; set; } // Product ID
        public string Image { get; set; } // Product image path
        public decimal Unit_Price { get; set; } // Unit price from ProductOption
        public decimal? Offer { get; set; } // Offer from ProductOption
        public int Quantity_Of_Unit { get; set; } // Quantity of units
        public decimal Subtotal { get; set; } // Subtotal for this product (Unit_Price * Quantity_Of_Unit - Offer * Quantity_Of_Unit)
    }
}
