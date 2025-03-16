using AlIssam.Api.Services.interFaces;
using AlIssam.API.Dtos.Response;
using AlIssam.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlIssam.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductOptionController(IproductOptionService service) : ControllerBase
    {
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int Id,ProductOptions request)
        {
            var result = await service.UpdateProductOptionAsync(Id, request);
            return HandleServiceResult(result);

        }

        private IActionResult HandleServiceResult<T>(
         (bool isSuccess, ErrorResponse? errorResponse, T? response) result)
        {
            if (!result.isSuccess || result.errorResponse != null)
                return BadRequest(result.errorResponse);

            return Ok(result.response);
        }
    }
}
