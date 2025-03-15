using AlIssam.API.Dtos;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlIssam.API.Services.interFaces
{
    public interface IProductService
    {
        Task<(bool isSuccess, ErrorResponse? errorMessage, int? productId)> CreateProductAsync(CreateProductRequest request);
        Task<(bool isSuccess, ErrorResponse? errorMessage, int? productId)> UpdateProductAsync(int id, UpdateProductRequest request);
        Task<(bool isSuccess, ErrorResponse? errorMessage, GetProductBySlugResponse? response)> GetProductBySlugAsync(string slug);
        Task<(bool isSuccess, ErrorResponse? errorMessage, List<GetProductResponse>? products)> SearchProductsAsync(string query, int? categoryId);
        Task<(bool isSuccess, ErrorResponse? errorMessage, string? message)> DeleteProductAsync(int id);
        Task<(bool isSuccess, ErrorResponse? errorMessage, List<GetProductResponse>? products)> GetProductsByCategorySlugAsync(string slug, int page);
        Task<(bool isSuccess, ErrorResponse? errorMessage, List<GetProductResponse>? products)> GetMostSellingProductsAsync();
        Task<(bool isSuccess, ErrorResponse? errorMessage, PagedList<GetProductResponse>? products)> GetAllProductsAsync(int page, int? categoryId, string? query);
    }
}