using Microsoft.EntityFrameworkCore;
using AlIssam.API.Dtos;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Extension;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlIssam.API.Services
{
    /// <summary>
    /// Manages discount codes and promotions
    /// </summary>
    public class DiscountService : IDiscountService
    {
        private readonly AlIssamDbContext _context;

        public DiscountService(AlIssamDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates a discount code and returns its active status and percentage
        /// </summary>
        /// <param name="code">The discount code to validate</param>
        /// <returns>A tuple containing success status, error message (if any), and discount details</returns>
        public async Task<(bool isSuccess, ErrorResponse? errorMessage, (bool Active, decimal Percent)? result)> IsValidDiscountAsync(string code)
        {
            var discount = await _context.Discounts
                .FirstOrDefaultAsync(c => c.Code == code);

            if (discount is null || discount.StarDate > DateTime.Now || discount.EndDate < DateTime.Now || !discount.Status)
                return (false, new ErrorResponse().CreateErrorResponse("Invalid discount code.", "كود الخصم غير صالح."), null);

            return (true, null, (discount.Status, discount.Percent));
        }

        public async Task<(bool isSuccess, ErrorResponse? errorMessage, PagedList<Discount>? result)> GetAllDiscountsAsync(int page)
        {
            const int pageSize = 20;

            var totalDiscounts = await _context.Discounts.CountAsync();

            var discounts = await _context.Discounts
                .OrderBy(d => d.DiscountId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedDiscounts = PagedList<Discount>.Create(discounts, page, pageSize, totalDiscounts);

            return (true, null, pagedDiscounts);
        }

        public async Task<(bool isSuccess, ErrorResponse? errorMessage, Discount? result)> GetDiscountByIdAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return (false, new ErrorResponse().CreateErrorResponse("Discount not found.", "الخصم غير موجود."), null);

            return (true, null, discount);
        }

        /// <summary>
        /// Creates a new discount code
        /// </summary>
        /// <param name="request">DTO containing discount creation parameters</param>
        /// <returns>A tuple containing success status, error message (if any), and the new discount ID</returns>
        public async Task<(bool isSuccess, ErrorResponse? errorMessage, int? discountId)> CreateDiscountAsync(CreateDiscountRequest request)
        {
            var discount = new Discount
            {
                Code = request.Code,
                StarDate = request.Start_Date,
                EndDate = request.End_Date,
                Percent = request.Percent,
                Status = true
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return (true, null, discount.DiscountId);
        }

        public async Task<(bool isSuccess, ErrorResponse? errorMessage, string? result)> UpdateDiscountAsync(int id, Discount updatedDiscount)
        {
            if (id != updatedDiscount.DiscountId)
                return (false, new ErrorResponse().CreateErrorResponse("ID mismatch.", "عدم تطابق المعرف."), null);

            var existingDiscount = await _context.Discounts.FindAsync(id);
            if (existingDiscount == null)
                return (false, new ErrorResponse().CreateErrorResponse("Discount not found.", "الخصم غير موجود."), null);

            existingDiscount.Code = updatedDiscount.Code;
            existingDiscount.StarDate = updatedDiscount.StarDate;
            existingDiscount.EndDate = updatedDiscount.EndDate;
            existingDiscount.Percent = updatedDiscount.Percent;
            existingDiscount.Status = updatedDiscount.Status;

            _context.Entry(existingDiscount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return (true, null, "Discount updated successfully.");
        }

        public async Task<(bool isSuccess, ErrorResponse? errorMessage, string? result)> DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return (false, new ErrorResponse().CreateErrorResponse("Discount not found.", "الخصم غير موجود."), null);

            discount.Status = false;
            await _context.SaveChangesAsync();

            return (true, null, "Discount deleted successfully.");
        }
    }
}