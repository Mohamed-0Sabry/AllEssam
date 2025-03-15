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
            try
            {
                
            var statusResponse = await _fatoorahService.GetPaymentStatusAsync(request.PaymentId);

            if (!statusResponse.IsSuccess || statusResponse.Data == null)
                return BadRequest("Invalid payment status");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.InvoiceId == statusResponse.Data.InvoiceId);

            if (order == null) return NotFound("Order not found");

            //statusResponse.Data.InvoiceStatus  = "Pending"
            //statusResponse.Data.InvoiceStatus == "Paid" &&
            if (order.InvoiceId == statusResponse.Data.InvoiceId )
            {
                order.Payment_Status_Ar = "مدفوع";
                order.Payment_Status_En = "Paid"; 
                order.Status_En = "Process"; 
                order.Status_Ar = "قيد المعالجة";

                await _context.SaveChangesAsync();
                return Redirect("http://164.68.102.161:8070/ar/cart?success_pay=true");
                //return Ok(new { Message = "Payment successful" });
            }

            BadRequest($"Payment verification failed Data:" +
                $" Response InoiveId: {statusResponse.Data.InvoiceId} " +
                $" Order InovideId  {order.InvoiceId}  " +
                $" Inovice Value : {statusResponse.Data.InvoiceValue}" +
                $"Order Total Amount Value {order.Total_Amount}" +
                $"Order Total_Amount_With_Discount Value {order.Total_Amount_With_Discount} ");
            return Redirect("http://26.97.71.252:3000/ar/cart?success_pay=false");
            }catch ( Exception ex ){
                BadRequest(" Payment verification failed " + ex.Message);
                return Redirect("https://alessam.store/ar/cart?success_pay=false");
            }
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
