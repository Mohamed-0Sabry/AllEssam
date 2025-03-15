using AlIssam.API.Dtos;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlIssam.API.Services
{
    public interface IDiscountService
    {
        Task<(bool isSuccess, ErrorResponse? errorMessage, (bool Active, decimal Percent)? result)> IsValidDiscountAsync(string code);
        Task<(bool isSuccess, ErrorResponse? errorMessage, PagedList<Discount>? result)> GetAllDiscountsAsync(int page);
        Task<(bool isSuccess, ErrorResponse? errorMessage, Discount? result)> GetDiscountByIdAsync(int id);
        Task<(bool isSuccess, ErrorResponse? errorMessage, int? discountId)> CreateDiscountAsync(CreateDiscountRequest request);
        Task<(bool isSuccess, ErrorResponse? errorMessage, string? result)> UpdateDiscountAsync(int id, Discount updatedDiscount);
        Task<(bool isSuccess, ErrorResponse? errorMessage, string? result)> DeleteDiscountAsync(int id);
    }
}