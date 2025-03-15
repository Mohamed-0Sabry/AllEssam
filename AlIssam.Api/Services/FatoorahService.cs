using AlIssam.Api.Config;
using AlIssam.Api.Services.interFaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AlIssam.Api.Services
{
    /// <summary>
    /// Integrates with MyFatoorah payment gateway for payment processing
    /// </summary>
    public class FatoorahService : IFatoorahService
    {
        private readonly HttpClient _httpClient;
        private readonly FatoorahConfig _configuration;
        public FatoorahService(IHttpClientFactory httpClientFactory, IOptions<FatoorahConfig> configuration)
        {
            _httpClient = httpClientFactory.CreateClient("FatoorahClient");
            _configuration = configuration.Value;
        }

        /// <summary>
        /// Initiates a payment request with MyFatoorah
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <returns>Payment initiation response from MyFatoorah</returns>
        /// <exception cref="HttpRequestException">Thrown when API communication fails</exception>
        public async Task<FatoorahResponse> SendPaymentAsync(PaymentRequest request)
        {
            return await CallFatoorahApiAsync<FatoorahResponse>("v2/SendPayment", request);
        }

        public async Task<ExecutePaymentResponse> ExecutePaymentAsync(ExecutePaymentRequest request)
        {
            return await CallFatoorahApiAsync<ExecutePaymentResponse>("v2/ExecutePayment", request);
        }

        public async Task<InitiatePaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request){
            return await CallFatoorahApiAsync<InitiatePaymentResponse>("v2/InitiatePayment", request);
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentId)
        {
            var requestData = new
            {
                Key = paymentId,
                KeyType = "PaymentId"
            };

            return await CallFatoorahApiAsync<PaymentStatusResponse>("v2/GetPaymentStatus", requestData).ConfigureAwait(false);
        }

        private async Task<T> CallFatoorahApiAsync<T>(string endpoint, object data)
        {
            var url = $"https://api.myfatoorah.com/{endpoint}";
            var json = JsonSerializer.Serialize(data);
            Console.WriteLine(json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.ApiToken);
            var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Fatoorah API Error: {response.StatusCode}\n{responseBody}");
            }

            Console.WriteLine("Response Body From MyFatoorah:");
            Console.WriteLine(responseBody);
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<T>(responseBody, options) ?? throw new InvalidOperationException("Failed to deserialize response.");
            }
            catch (JsonException ex)
            {
                throw new HttpRequestException("Failed to parse MyFatoorah API response.", ex);
            }
        }
    }

    public class PaymentRequest
    {
        public string? CustomerName { get; set; }
        public string? NotificationOption { get; set; } = "LNK";
        public decimal InvoiceValue { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CallBackUrl { get; set; }
        public string? ErrorUrl { get; set; }
        public string? Language { get; set; } = "en";
        public string? DisplayCurrencyIso { get; set; } = "KWD";
    }

    public class FatoorahResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public PaymentData? Data { get; set; }
    }

    public class PaymentData
    {
        public int InvoiceId { get; set; }
        public string? InvoiceURL { get; set; }
    }

    public class PaymentStatusResponse
    {
        public bool IsSuccess { get; set; }
        public PaymentStatusData Data { get; set; }
    }

    public class PaymentStatusData
    {
        public int InvoiceId { get; set; }
        public string? InvoiceStatus { get; set; }
        public decimal InvoiceValue { get; set; }
    }

    public class ExecutePaymentRequest
    {
        public decimal InvoiceValue { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? CustomerName { get; set; }
        public string? CallBackUrl { get; set; }
        public string? ErrorUrl { get; set; }
        public string? DisplayCurrencyIso { get; set; }
        public string? MobileCountryCode { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Language { get; set; }
        public string? CustomerReference { get; set; }
        public CustomerAddressModel? CustomerAddress { get; set; }
        public List<InvoiceItemModel>? InvoiceItems { get; set; }
    }

    public class CustomerAddressModel
    {
        public string? Block { get; set; }
        public string? Street { get; set; }
        public string? HouseBuildingNo { get; set; }
        public string? AddressInstructions { get; set; }
    }

    public class InvoiceItemModel
    {
        public string? ItemName { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }

    public class ExecutePaymentResponse
    {
        public int InvoiceId { get; set; }
        public bool IsDirectPayment { get; set; }
        public string PaymentURL { get; set; }
        public string CustomerReference { get; set; }
    }

    public class InitiatePaymentRequest
    {
        public decimal InvoiceAmount { get; set; }
        public string CurrencyIso { get; set; }
    }
    public class InitiatePaymentResponse
    {
        public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    }
    public class PaymentMethod
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodEn { get; set; }
    }
    
    public class PaymentMethodResponse
    {
        public int? VisaMasterId;
        public int? KnetId;

    }
}
