using System.Threading.Tasks;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos;

namespace AlIssam.API.Services
{
    public interface IOrderService
    {
        Task<(bool isSuccess, ErrorResponse? errorMessage, GetOrderDataResponse? result)> GetOrderByIdAsync(string id);
        Task<(bool isSuccess, ErrorResponse? errorMessage, string? paymentUrl)> CreateOrderAsync(CreateOrderRequest orderCreateDto, string userId);
        Task<(bool isSuccess, ErrorResponse? errorMessage, UpdateStatusResponse? result)> UpdateOrderStatusAsync(string id, UpdateStatusRequest request);
        Task<(bool isSuccess, ErrorResponse? errorMessage, PagedList<GetOrderDataResponse>? result)> GetAllOrdersAsync(string? status, string? userName, int page, string? userId = null);
    }
}