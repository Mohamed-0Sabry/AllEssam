using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Request
{
    public class CreateOrderRequest
    {
        [JsonPropertyName("Code")]
        public string?  Code { get; set; }

        [JsonPropertyName("Location")]
        public string Location  { get; set; }

        [JsonPropertyName("Recevier")]
        public string Recevier { get; set; }

        [JsonPropertyName("Phone")]
        public string Phone { get; set; }

        [JsonPropertyName("Order_Details")]
        public List<CreateOrderDetailRequest> Order_Details { get; set; } // List of order details

        [JsonPropertyName("Payment_Method")]
        public string Payment_method { get; set; }

    }
}
