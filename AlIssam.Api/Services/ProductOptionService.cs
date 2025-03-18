using AlIssam.Api.Services.interFaces;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Extension;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.Api.Services
{
    public class ProductOptionService(AlIssamDbContext dbContext) : IproductOptionService
    {
        public async Task<(bool isSuccess, ErrorResponse? errorMessage, int? productOptionId)> UpdateProductOptionAsync(int id, ProductOptions request)
        {
            var option = await dbContext.ProductsOptions.FirstOrDefaultAsync(x => x.Id == id);

            if (option is null)
                return (false, new ErrorResponse().CreateErrorResponse("لا يوجد خيارات للمنتج بهذا المعرف", "not found"), null);


            option.Offer = request.Offer;
            option.Price = request.Price;
            option.OrderDetailsList = request.OrderDetailsList;
            option.Default = request.Default;

            dbContext.ProductsOptions.Update(option);

            if (await dbContext.SaveChangesAsync() > 0) 
            {
                return (true, null, id);
            }

            return(false, new ErrorResponse().CreateErrorResponse("حدث خطأ", "An error occurred ."), null);

        }
    }
}
