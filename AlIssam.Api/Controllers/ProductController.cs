using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using AlIssam.API.Dtos;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Services.interFaces;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace AlIssam.API.Controllers
{
    /// <summary>
    /// Manages product catalog and inventory
    /// </summary>
    public class ProductController : ControllerBase
    {
        private readonly AlIssamDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IProductService _productService;

        public ProductController(AlIssamDbContext context, IFileStorageService fileStorageService, IProductService productService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _productService = productService;
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="request">Product data with images</param>
        /// <returns>Created product details</returns>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid product data</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden (non-admin users)</response>
        [HttpPost("api/product/create")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request)
        {
            if (string.IsNullOrEmpty(request.Name_Ar) || string.IsNullOrEmpty(request.Name_En))
                return BadRequest("Arabic name and English name are required.");

            var form = await Request.ReadFormAsync();

            request.Quantities = ExtractFormData<ProductOptions>(
                        form,
                        "Quantities",
                        value => JsonConvert.DeserializeObject<ProductOptions>(value)
            );

            request.Categories_Ids = ExtractFormData<int>(
                        form,
                        "Category_Id",
                        value => int.TryParse(value, out int categoryId) ? categoryId : throw new ArgumentException("Invalid category ID")
                    );

            request.ProductImages = ExtractFiles(form, "Images");

            var result = await _productService.CreateProductAsync(request);

            return HandleServiceResult(result);
        }

        /// <summary>
        /// Search products by name or category
        /// </summary>
        /// <param name="query">Search keywords</param>
        /// <param name="categoryId">Optional category filter</param>
        /// <returns>List of matching products</returns>
        /// <response code="200">Returns product list</response>
        [HttpGet("api/product/search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] int? categoryId)
        {
            var result = await _productService.SearchProductsAsync(query, categoryId);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// Get product details by URL slug
        /// </summary>
        /// <param name="slug">Product URL slug</param>
        /// <returns>Complete product details with similar items</returns>
        /// <response code="200">Returns product details</response>
        /// <response code="404">Product not found</response>
        [HttpGet("api/product/{slug}")]
        public async Task<IActionResult> GetProductBySlug(string slug)
        {
            var result = await _productService.GetProductBySlugAsync(slug);
            return HandleServiceResult(result);
        }

        [HttpPost("api/products/{id}")]
        public async Task<IActionResult> UpdateProduct1(int id, [FromForm] UpdateProductRequest request)
        {
            var form = await Request.ReadFormAsync();
            request.Quantities = ExtractFormData<ProductOptions>
                (
                form,
                "Quantities",
                value => JsonConvert.DeserializeObject<ProductOptions>(value)
                );

            request.Categories_Ids = ExtractFormData<int>
              (
              form,
              "Category_Id",
                value => int.TryParse(value, out int result) ? result : throw new ArgumentException("Invalid integer value for Category.")
              );

            request.Product_New_Images = ExtractFiles(form, "Images");
            request.Product_Images_Paths = form.Keys
                    .Where(key => key.StartsWith("Images") && form[key].Count == 1 && !string.IsNullOrEmpty(form[key][0]))
                    .Select(key => form[key][0])
                    .OfType<string>() // Ensure the value is a string
                    .ToList();

            var result = await _productService.UpdateProductAsync(id, request);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// Archive a product (Admin only)
        /// </summary>
        /// <param name="id">Product ID to archive</param>
        /// <returns>Success message</returns>
        /// <response code="200">Product archived</response>
        /// <response code="404">Product not found</response>
        [HttpPost("api/product/delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return HandleServiceResult(result);
        }

        [HttpGet("api/products/category/{slug}")]
        public async Task<IActionResult> GetProductsByCategorySlug(string slug, [FromQuery] int page = 1)
        {
            var result = await _productService.GetProductsByCategorySlugAsync(slug, page);
            return HandleServiceResult(result);
        }

        [HttpGet("api/products/mostselling")]
        public async Task<IActionResult> GetMostSellingProducts()
        {
            var result = await _productService.GetMostSellingProductsAsync();
            return HandleServiceResult(result);
        }


        [HttpGet("api/products")]
        public async Task<IActionResult> GetAllProducts(
           [FromQuery] int page = 1,
           [FromQuery] int? categoryid = null,
           [FromQuery] string? query = null)
        {
            var result = await _productService.GetAllProductsAsync(page, categoryid, query);
            return HandleServiceResult(result);
        }

        private IActionResult HandleServiceResult<T>(
            (bool isSuccess, ErrorResponse? errorResponse, T? response) result)
        {
            if (!result.isSuccess || result.errorResponse != null)
                return BadRequest(result.errorResponse);

            return Ok(result.response);
        }
        private static List<T> ExtractFormData<T>(
            IFormCollection form,
            string keyPrefix,
            Func<string, T> converter)
        {
            var result = new List<T>();

            foreach (var key in form.Keys)
            {
                if (key.StartsWith(keyPrefix))
                {
                    var value = form[key];
                    var convertedValue = converter(value);
                    result.Add(convertedValue);
                }
            }

            return result;
        }

        private static List<IFormFile> ExtractFiles(IFormCollection form, string keyPrefix)
        {
            var files = new List<IFormFile>();

            // Iterate through all files in the form
            foreach (var file in form.Files)
            {
                // Check if the file name starts with the key prefix and matches the exact pattern (e.g., "Images0", "Images1", etc.)
                if (file.Name.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase) &&
                    file.Name.Length > keyPrefix.Length && // Ensure there's something after the prefix
                    int.TryParse(file.Name.Substring(keyPrefix.Length), out _)) // Ensure the suffix is a number
                {
                    files.Add(file);
                }
            }

            return files;
        }
    }
}