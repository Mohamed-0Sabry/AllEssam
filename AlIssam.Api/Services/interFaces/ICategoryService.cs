using AlIssam.API.Dtos;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.DataAccessLayer.Entities;
using System.Threading.Tasks;

namespace AlIssam.API.Services.interFaces
{
    public interface ICategoryService
    {
        Task<(bool isSuccess, ErrorResponse? errorMessage, Category? response)> GetCategoryByIdAsync(int id);
        Task<(bool isSuccess, ErrorResponse? errorMessage, Category? response)> GetCategoryBySlugAsync(string slug);
        Task<(bool isSuccess, ErrorResponse? errorMessage, Category? response)> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<(bool isSuccess, ErrorResponse? errorMessage, string? response)> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, int id);
        Task<(bool isSuccess, ErrorResponse? errorMessage, object? response)> GetAllCategoriesAsync(
            int page = 1,
            string? query = null);
        Task<(bool isSuccess, ErrorResponse? errorMessage, object? response)> DeleteCategoryAsync(int id);
    }
}