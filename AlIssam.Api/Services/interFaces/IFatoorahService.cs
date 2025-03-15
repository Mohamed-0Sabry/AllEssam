namespace AlIssam.Api.Services.interFaces
{
    public interface IFatoorahService
    {
        public Task<FatoorahResponse> SendPaymentAsync(PaymentRequest request);
        public Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentId);
        public Task<ExecutePaymentResponse> ExecutePaymentAsync(ExecutePaymentRequest request);
        public  Task<InitiatePaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request);

    }
}
