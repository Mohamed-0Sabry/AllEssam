using AlIssam.Api.Services;
using AlIssam.Api.Services.interFaces;
using AlIssam.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlIssam.Api.Controllers
{
    public class PaymentController : ControllerBase
    {

        private readonly IFatoorahService _fatoorahService;
        private readonly  AlIssamDbContext  _context;

        public PaymentController(IFatoorahService fatoorahService, AlIssamDbContext context)
        {
            _fatoorahService = fatoorahService;
            _context = context;
        }

        [HttpPost("initiate")]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
        {
            try
            {
                var response = await _fatoorahService.InitiatePaymentAsync(request);
                Console.WriteLine(response.ToString());
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Payment initiation failed", error = ex.Message });
            }
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecutePayment([FromBody] ExecutePaymentRequest request)
        {
            try
            {
                if (request.PaymentMethodId == null)
                {
                    return BadRequest("Either PaymentMethodId or SessionId must be provided.");
                }

                var response = await _fatoorahService.ExecutePaymentAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Payment execution failed", error = ex.Message });
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] PaymentCallbackRequest request)
        {
            var statusResponse = await _fatoorahService.GetPaymentStatusAsync(request.PaymentId);

            if (!statusResponse.IsSuccess || statusResponse.Data == null)
                return BadRequest("Invalid payment status");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.InvoiceId == statusResponse.Data.InvoiceId);

            if (order == null) return NotFound("Order not found");

            if (statusResponse.Data.InvoiceStatus == "Paid" &&
                (order.Total_Amount == statusResponse.Data.InvoiceValue ||
                 order.Total_Amount_With_Discount == statusResponse.Data.InvoiceValue ))
            {
                order.Payment_Status_Ar = "مدفوع";
                order.Payment_Status_En = "paid";

                await _context.SaveChangesAsync();
                return Redirect("https://alshamiaa.com/ar/cart?success_pay=true");
            }

            return BadRequest($"Payment verification failed Data:" +
                $" Response InoiveId: {statusResponse.Data.InvoiceId} " +
                $" Order InovideId  {order.InvoiceId}  " +
                $" Inovice Value : {statusResponse.Data.InvoiceValue}" +
                $"Order Total Amount Value {order.Total_Amount}" +
                $"Order Total_Amount_With_Discount Value {order.Total_Amount_With_Discount} ");
        }
    }

    // CheckoutRequest.cs
    public class CheckoutRequest
    {
        public string CustomerName { get; set; }
        public decimal TotalPrice { get; set; }
        public string CustomerEmail { get; set; }
        // Add other necessary properties from your order form
    }

    // PaymentCallbackRequest.cs
    public class PaymentCallbackRequest
    {
        public string PaymentId { get; set; }
    }

 
}
