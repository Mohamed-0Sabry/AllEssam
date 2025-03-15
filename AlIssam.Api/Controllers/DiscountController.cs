using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlIssam.API.Dtos;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Services;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlIssam.API.Controllers
{
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        // Helper method to handle service results
        private IActionResult HandleServiceResult<T>(
            (bool isSuccess, ErrorResponse? errorResponse, T? response) result)
        {
            if (!result.isSuccess || result.errorResponse != null)
                return BadRequest(result.errorResponse);

            return Ok(result.response);
        }

        // GET: api/discount/check/{code}
        [HttpGet("api/discount/check/{code}")]
        public async Task<IActionResult> IsValid(string code)
        {
            var result = await _discountService.IsValidDiscountAsync(code);
            Console.WriteLine(result);
            Console.WriteLine(result);

            if (!result.isSuccess)
                return NotFound();
            var res = new { Active = result.result.Value.Active, Percent = result.result.Value.Percent };
            return Ok(res);
        }

        // GET: api/discounts
        [HttpGet("api/discounts")]
        public async Task<IActionResult> GetAllDiscounts([FromQuery] int page = 1)
        {
            var result = await _discountService.GetAllDiscountsAsync(page);
            return HandleServiceResult(result);
        }

        // GET: api/discount/{id}
        [HttpGet("api/discount/{id}")]
        public async Task<IActionResult> GetDiscountById(int id)
        {
            var result = await _discountService.GetDiscountByIdAsync(id);
            return HandleServiceResult(result);
        }

        // POST: api/discount/create
        [HttpPost("api/discount/create")]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request)
        {
            var result = await _discountService.CreateDiscountAsync(request);
            return HandleServiceResult(result);
        }

        // PUT: api/discount/{id}
        [HttpPut("api/discount/{id}")]
        public async Task<IActionResult> UpdateDiscount(int id, [FromBody] Discount updatedDiscount)
        {
            var result = await _discountService.UpdateDiscountAsync(id, updatedDiscount);
            return HandleServiceResult(result);
        }

        // DELETE: api/discount/{id}
        [HttpPost("api/discount/delete/{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var result = await _discountService.DeleteDiscountAsync(id);
            return HandleServiceResult(result);
        }

        //// GET: api/discount/check/{code}
        //[HttpGet("api/discount/check/{code}")]
        //public async Task<IActionResult> IsValid(string code)
        //{
        //    var discount = await _context.Discounts
        //        .FirstOrDefaultAsync(c => c.Code == code);

        //    if (discount is null || discount.StarDate > DateTime.Now || discount.EndDate < DateTime.Now || !discount.Status)
        //        return BadRequest(new { Active = false, Percent = discount?.Percent });

        //    return Ok(new { Active = discount.Status, Percent = discount.Percent });
        //}

        //// GET: api/discount
        //[HttpGet("api/discounts")]
        //public async Task<IActionResult> GetAllDiscounts(
        //    [ FromQuery] int page = 1) // Default page number is 1
        //{
        //    // Hardcoded page size
        //    const int pageSize = 20; // Number of items per page

        //    // Get the total count of discounts
        //    var totalDiscounts = await _context.Discounts.CountAsync();

        //    // Apply pagination
        //    var discounts = await _context.Discounts
        //        .OrderBy(d => d.DiscountId) // Order by ID (or any other field)
        //        .Skip((page - 1) * pageSize) // Skip items on previous pages
        //        .Take(pageSize) // Take items for the current page
        //        .ToListAsync();

        //    var pagedDiscounts = PagedList<Discount>.Create(discounts, page, pageSize, totalDiscounts);

        //    return Ok(pagedDiscounts);
        //}

        //// GET: api/discount/{id}
        //[HttpGet("api/discount/{id}")]
        //public async Task<IActionResult> GetDiscountById(int id)
        //{
        //    var discount = await _context.Discounts.FindAsync(id);
        //    if (discount == null)
        //        return NotFound("Discount not found.");

        //    return Ok(discount);
        //}

        //[HttpPost("api/discount/create")]
        //public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // Map the request to a Discount entity
        //    var discount = new Discount
        //    {
        //        Code = request.Code,
        //        StarDate = request.Start_Date,
        //        EndDate = request.End_Date,   
        //        Percent = request.Percent,
        //        Status = true 
        //    };

        //    _context.Discounts.Add(discount);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetDiscountById), new { id = discount.DiscountId }, discount);
        //}

        //// PUT: api/discount/{id}
        //[HttpPut("api/discount/{id}")]
        //public async Task<IActionResult> UpdateDiscount(int id, [FromBody] Discount updatedDiscount)
        //{
        //    if (id != updatedDiscount.DiscountId)
        //    {
        //        return BadRequest("ID mismatch.");
        //    }

        //    var existingDiscount = await _context.Discounts.FindAsync(id);
        //    if (existingDiscount == null)
        //    {
        //        return NotFound("Discount not found.");
        //    }

        //    existingDiscount.Code = updatedDiscount.Code;
        //    existingDiscount.StarDate = updatedDiscount.StarDate;
        //    existingDiscount.EndDate = updatedDiscount.EndDate;
        //    existingDiscount.Percent = updatedDiscount.Percent;
        //    existingDiscount.Status = updatedDiscount.Status;

        //    _context.Entry(existingDiscount).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //// DELETE: api/discount/{id}
        //[HttpDelete("api/discount/{id}")]
        //public async Task<IActionResult> DeleteDiscount(int id)
        //{
        //    var discount = await _context.Discounts.FindAsync(id);
        //    if (discount == null)
        //        return NotFound("Discount not found.");

        //    discount.Status = false;
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
    }
}