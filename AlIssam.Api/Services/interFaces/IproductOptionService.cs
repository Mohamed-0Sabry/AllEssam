using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.Api.Services.interFaces
{
    public interface IproductOptionService
    {
        Task<(bool isSuccess, ErrorResponse? errorMessage, int? productOptionId)> UpdateProductOptionAsync(int id, ProductOptions request);

    }
}
