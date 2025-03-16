using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using AlIssam.Api.Services;
using AlIssam.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using AlIssam.API.Common;
using AlIssam.API.Services.InterFaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlIssam.Api.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly AlIssamDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(AlIssamDbContext context, IConfiguration configuration, IEmailService emailService, ILogger<WebhookController> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Handles MyFatoorah webhook events.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] GenericWebhookModel<WebhookTransactionStatus>? model)
        {
            if (model == null || model.Data == null)
            {
                _logger.LogWarning("Received an invalid webhook payload.");
                return BadRequest(new { error = "Invalid Payload. Ensure the request body matches the expected format." });
            }

            try
            {
                var signatureHeader = Request.Headers["MyFatoorah-Signature"].ToString();
                var secretKey = _configuration["Fatoorah:SecretKey"];

                _logger.LogInformation($"Received Webhook: {JsonConvert.SerializeObject(model, Formatting.Indented)}");
                _logger.LogInformation($"Received Signature: {signatureHeader}");
                

                if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(signatureHeader) ||
                    !CheckMyFatoorahSignature(model, secretKey, signatureHeader))
                {
                    _logger.LogWarning("Invalid Signature for webhook event.");
                    return BadRequest("Invalid Signature");
                }

                switch (model.EventType)
                {
                    case WebhookEvents.TransactionsStatusChanged:
                        await HandleTransactionStatus(model.Data);
                        break;

                    default:
                        _logger.LogWarning($"Unhandled Webhook Event: {model.EventType}");
                        break;
                }

                return Ok("Webhook processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Processing Webhook: {ex.Message}");
                return BadRequest("Internal Server Error");
            }
        }

        /// <summary>
        /// Handles transaction status updates.
        /// </summary>
        private async Task HandleTransactionStatus(WebhookTransactionStatus data)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.InvoiceId == data.InvoiceId);

            if (order != null)
            {
                _logger.LogInformation($"Updating Order {order.Id} with status {data.TransactionStatus}");
                
                order.Payment_Status_En = "Paid";
                order.Payment_Status_Ar = "مدفوع";
                order.IsPaid = true;
                await _context.SaveChangesAsync();

                var userName = order.User?.UserName ?? "Unknown User";

                string emailContent = $"Transaction Data: {JsonConvert.SerializeObject(data)}\nOrder Data: {JsonConvert.SerializeObject(order)}";
                var emailMessage = new Message(userName, "mahm192005@gmail.com", "MyFatoorah Webhook Response", emailContent);

                if (_emailService == null)
                {
                    _logger.LogError("Email service is not initialized!");
                    return;
                }

                try{
                    _emailService.SendEmail(emailMessage);
                }catch (Exception ex){
                    _logger.LogError($"Failed to send email: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning($"Order not found for InvoiceId {data.InvoiceId}");
            }
        }

        /// <summary>
        /// Validates MyFatoorah webhook signature.
        /// </summary>
        private bool CheckMyFatoorahSignature(GenericWebhookModel<WebhookTransactionStatus> model, string secretKey, string headerSignature)
        {
            var properties = model.Data.GetType().GetProperties()
                            .Select(p => p.Name)
                            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                            .ToList();

            var parameters = properties.Select(property =>
                new ItemTxt { Text = property, Value = model.Data.GetType().GetProperty(property)?.GetValue(model.Data)?.ToString() ?? "" }
            ).ToList();

            var computedSignature = Sign(parameters, secretKey);

            _logger.LogWarning($"Received Signature: {headerSignature}");
            _logger.LogWarning($"Computed Signature: {computedSignature}");

            return computedSignature == headerSignature;
        }

        /// <summary>
        /// Generates HMAC SHA256 signature.
        /// </summary>
        private static string Sign(List<ItemTxt> paramsArray, string secretKey)
        {
            var dataToSign = paramsArray.Select(p => $"{p.Text}={p.Value}").ToList();
            var data = string.Join(",", dataToSign);
            var encoding = new UTF8Encoding();
            var keyByte = encoding.GetBytes(secretKey);
            using var hmacsha256 = new HMACSHA256(keyByte);
            var messageBytes = encoding.GetBytes(data);
            return Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
        }
    }

    /// <summary>
    /// Generic Webhook Model.
    /// </summary>
    public class GenericWebhookModel<T>
    {
        public WebhookEvents EventType { get; set; }
        public string? Event { get; set; }
        public string? DateTime { get; set; }
        public string? CountryIsoCode { get; set; }
        public T? Data { get; set; }
    }

    public enum WebhookEvents
    {
        TransactionsStatusChanged = 1,
        RefundStatusChanged = 2,
        BalanceTransferred = 3,
        SupplierStatusChanged = 4
    }

    public class WebhookTransactionStatus
    {
        [JsonConverter(typeof(ParseIntConverter))]
        public int InvoiceId { get; set; }
        public string? InvoiceReference { get; set; }
        public string? CreatedDate { get; set; }
        public string? CustomerReference { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string? TransactionStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? UserDefinedField { get; set; }
        public string? ReferenceId { get; set; }
        public string? TrackId { get; set; }
        public string? PaymentId { get; set; }
        public string? AuthorizationId { get; set; }
        public string? InvoiceValueInBaseCurrency { get; set; }
        public string? BaseCurrency { get; set; }
        public string? InvoiceValueInDisplayCurrency { get; set; }
        public string? DisplayCurrency { get; set; }
        public string? InvoiceValueInPayCurrency { get; set; }
        public string? PayCurrency { get; set; }
    }

    /// <summary>
    /// Allows InvoiceId to be converted from string to int.
    /// </summary>
    public class ParseIntConverter : JsonConverter<int>
    {
        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return int.TryParse(reader.Value?.ToString(), out int value) ? value : 0;
        }

        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }

    public class ItemTxt
    {
        public string? Value { get; set; }
        public string? Text { get; set; }
    }
}
