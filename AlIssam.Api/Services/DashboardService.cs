using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;
using AlIssam.API.Dtos.Response;
using System;
using System.Linq;
using System.Threading.Tasks;
using AlIssam.API.Extension;

namespace AlIssam.API.Services
{
    /// <summary>
    /// Provides business intelligence and reporting functionality including statistics and data exports
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly AlIssamDbContext _context;

        public DashboardService(AlIssamDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves key performance indicators and business metrics
        /// </summary>
        /// <param name="year">Optional year filter</param>
        /// <param name="month">Optional month filter</param>
        /// <returns>A tuple containing success status, error message (if any), and dashboard statistics</returns>
        public async Task<(bool isSuccess, ErrorResponse? errorResponse, DashboardStatisticsResponse? response)> GetDashboardStatisticsAsync(int? year, int? month)
        {
            // Validate year and month inputs
            if (year.HasValue && (year < 1900 || year > DateTime.UtcNow.Year))
                return (false, new ErrorResponse().CreateErrorResponse("Invalid year. Year must be between 1900 and the current year.", "سنة غير صالحة. يجب أن تكون السنة بين 1900 والسنة الحالية."), null);

            if (month.HasValue && (month < 1 || month > 12))
                return (false, new ErrorResponse().CreateErrorResponse("Invalid month. Month must be between 1 and 12.", "شهر غير صالح. يجب أن يكون الشهر بين 1 و 12."), null);

            // Calculate start and end dates based on year and month
            var (startDate, endDate) = GetDateRange(year, month);

            // Get total users (filtered by date if applicable)
            var totalUsers = await GetFilteredUserCountAsync(startDate, endDate);

            // Get completed orders (filtered by date if applicable)
            var completedOrders = await GetOrderCountByStatusAsync("Completed", startDate, endDate);

            // Get cancelled orders (filtered by date if applicable)
            var cancelledOrders = await GetOrderCountByStatusAsync("Cancelled", startDate, endDate);

            // Calculate total payment and profit (example logic, adjust as needed)
            var totalPayment = await GetFilteredOrderSumAsync(startDate, endDate);

            var profit = totalPayment * 0.2m; // Example: 20% profit

            // Prepare response
            var response = new DashboardStatisticsResponse
            {
                Total_Users = totalUsers,
                Completed_Orders = completedOrders,
                Cancelled_Orders = cancelledOrders,
                Total_Payment = totalPayment,
                Profit = profit,
            };

            return (true, null, response);
        }

        /// <summary>
        /// Exports dashboard statistics to Excel format
        /// </summary>
        /// <param name="year">Optional year filter</param>
        /// <param name="month">Optional month filter</param>
        /// <returns>A tuple containing success status, error message (if any), and Excel file bytes</returns>
        public async Task<(bool isSuccess, ErrorResponse? errorResponse, byte[]? fileBytes)> ExportDashboardStatisticsToExcelAsync(int? year, int? month)
        {
            // Validate year and month inputs
            if (year.HasValue && (year < 1900 || year > DateTime.UtcNow.Year))
            {
                return (false, new ErrorResponse().CreateErrorResponse("Invalid year. Year must be between 1900 and the current year.", "سنة غير صالحة. يجب أن تكون السنة بين 1900 والسنة الحالية."), null);
            }

            if (month.HasValue && (month < 1 || month > 12))
            {
                return (false, new ErrorResponse().CreateErrorResponse("Invalid month. Month must be between 1 and 12.", "شهر غير صالح. يجب أن يكون الشهر بين 1 و 12."), null);
            }

            // Calculate start and end dates based on year and month
            var (startDate, endDate) = GetDateRange(year, month);

            // Query the database to get statistics
            var totalUsers = await GetFilteredUserCountAsync(startDate, endDate);
            var completedOrders = await GetOrderCountByStatusAsync("Completed", startDate, endDate);
            var cancelledOrders = await GetOrderCountByStatusAsync("Cancelled", startDate, endDate);
            var totalPayment = await GetFilteredOrderSumAsync(startDate, endDate);
            var profit = totalPayment * 0.2m; // Example: 20% profit

            // Create an Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Dashboard Statistics");

                // Add headers and data
                worksheet.Cells[1, 1].Value = "Metric";
                worksheet.Cells[1, 2].Value = "Value";

                worksheet.Cells[2, 1].Value = "Total Users";
                worksheet.Cells[2, 2].Value = totalUsers;

                worksheet.Cells[3, 1].Value = "Completed Orders";
                worksheet.Cells[3, 2].Value = completedOrders;

                worksheet.Cells[4, 1].Value = "Cancelled Orders";
                worksheet.Cells[4, 2].Value = cancelledOrders;

                worksheet.Cells[5, 1].Value = "Total Payment";
                worksheet.Cells[5, 2].Value = totalPayment;
                worksheet.Cells[5, 2].Style.Numberformat.Format = "$#,##0.00";

                worksheet.Cells[6, 1].Value = "Profit";
                worksheet.Cells[6, 2].Value = profit;
                worksheet.Cells[6, 2].Style.Numberformat.Format = "$#,##0.00";

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the package to a byte array
                var fileBytes = package.GetAsByteArray();
                return (true, null, fileBytes);
            }
        }

        public async Task<(bool isSuccess, ErrorResponse? errorResponse, byte[]? fileBytes)> ExportUsersToExcelAsync()
        {
            // Query the database to get user data
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserName,
                    u.Email,
                    u.City,
                    Total_Orders = u.Orders.Count
                })
                .ToListAsync();

            // Create an Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Add headers and data
                worksheet.Cells[1, 1].Value = "User Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Total Orders";
                worksheet.Cells[1, 4].Value = "City";

                for (int i = 0; i < users.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = users[i].UserName;
                    worksheet.Cells[i + 2, 2].Value = users[i].Email;
                    worksheet.Cells[i + 2, 3].Value = users[i].Total_Orders;
                    worksheet.Cells[i + 2, 4].Value = users[i].City;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the package to a byte array
                var fileBytes = package.GetAsByteArray();
                return (true, null, fileBytes);
            }
        }

        public async Task<(bool isSuccess, ErrorResponse? errorResponse, byte[]? fileBytes)> ExportProductsToExcelAsync()
        {
            // Query the database to get product data including options
            var products = await _context.Products
                .Include(p => p.Quantity)
                .Select(p => new
                {
                    p.Id,
                    p.Name_Ar,
                    p.Name_En,
                    p.Description_Ar,
                    p.Description_En,
                    p.Slug_Ar,
                    p.Slug_En,
                    p.IsEnded,
                    Total_Images = p.ProductsImages.Count,
                    Options = p.Quantity.Select(q => new
                    {
                        q.Id,
                        Name_Ar = p.Unit_Of_Measurement_Ar,
                        Name_En = p.Unit_Of_Measurement_En,
                        q.Price,
                        q.Offer,
                        q.Default
                    }).ToList()
                })
                .ToListAsync();

            // Create an Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");

                // Add headers and data
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name (Arabic)";
                worksheet.Cells[1, 3].Value = "Name (English)";
                worksheet.Cells[1, 4].Value = "Description (Arabic)";
                worksheet.Cells[1, 5].Value = "Description (English)";
                worksheet.Cells[1, 6].Value = "Slug (Arabic)";
                worksheet.Cells[1, 7].Value = "Slug (English)";
                worksheet.Cells[1, 8].Value = "Is Ended";
                worksheet.Cells[1, 9].Value = "Total Images";

                int maxOptions = products.Max(p => p.Options.Count);
                int col = 10;
                for (int i = 1; i <= maxOptions; i++)
                {
                    worksheet.Cells[1, col++].Value = $"Option{i}_Id";
                    worksheet.Cells[1, col++].Value = $"Option{i}_Name_Ar";
                    worksheet.Cells[1, col++].Value = $"Option{i}_Name_En";
                    worksheet.Cells[1, col++].Value = $"Option{i}_Price";
                    worksheet.Cells[1, col++].Value = $"Option{i}_Offer";
                    worksheet.Cells[1, col++].Value = $"Option{i}_Default";
                }

                int row = 2;
                foreach (var product in products)
                {
                    worksheet.Cells[row, 1].Value = product.Id;
                    worksheet.Cells[row, 2].Value = product.Name_Ar;
                    worksheet.Cells[row, 3].Value = product.Name_En;
                    worksheet.Cells[row, 4].Value = product.Description_Ar;
                    worksheet.Cells[row, 5].Value = product.Description_En;
                    worksheet.Cells[row, 6].Value = product.Slug_Ar;
                    worksheet.Cells[row, 7].Value = product.Slug_En;
                    worksheet.Cells[row, 8].Value = product.IsEnded;
                    worksheet.Cells[row, 9].Value = product.Total_Images;

                    col = 10;
                    foreach (var option in product.Options)
                    {
                        worksheet.Cells[row, col++].Value = option.Id;
                        worksheet.Cells[row, col++].Value = option.Name_Ar;
                        worksheet.Cells[row, col++].Value = option.Name_En;
                        worksheet.Cells[row, col++].Value = option.Price;
                        worksheet.Cells[row, col++].Value = option.Offer;
                        worksheet.Cells[row, col++].Value = option.Default;
                    }

                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the package to a byte array
                var fileBytes = package.GetAsByteArray();
                return (true, null, fileBytes);
            }
        }

        public async Task<(bool isSuccess, ErrorResponse? errorResponse, byte[]? fileBytes)> ExportOrdersToExcelAsync()
        {
            // Query the database to get order data including order details
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrdersDetails)
                    .ThenInclude(od => od.Product)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Status_En,
                    o.Status_Ar,
                    User_Name = o.User.UserName,
                    OrderDetails = o.OrdersDetails.Select(od => new
                    {
                        od.Id,
                        od.ProductId,
                        od.Unit_Price,
                        od.Offer,
                        od.Quantity_Of_Unit,
                        Product_Name_Ar = od.Product.Name_Ar,
                        Product_Name_En = od.Product.Name_En
                    }).ToList()
                })
                .ToListAsync();

            // Create an Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Orders");

                // Add headers and data
                worksheet.Cells[1, 1].Value = "Order ID";
                worksheet.Cells[1, 2].Value = "Order Date";
                worksheet.Cells[1, 3].Value = "Status (English)";
                worksheet.Cells[1, 4].Value = "Status (Arabic)";
                worksheet.Cells[1, 5].Value = "User Name";

                int maxOrderDetails = orders.Max(o => o.OrderDetails.Count);
                int col = 6;
                for (int i = 1; i <= maxOrderDetails; i++)
                {
                    worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Id";
                    worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Product_Id";
                    worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Product_Name_Ar";
                    worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Product_Name_En";
                    worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Unit_Price";
                    worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Offer";
                    worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Quantity_Of_Unit";
                }

                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cells[row, 1].Value = order.Id;
                    worksheet.Cells[row, 2].Value = order.OrderDate;
                    worksheet.Cells[row, 3].Value = order.Status_En;
                    worksheet.Cells[row, 4].Value = order.Status_Ar;
                    worksheet.Cells[row, 5].Value = order.User_Name;

                    col = 6;
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        worksheet.Cells[row, col++].Value = orderDetail.Id;
                        worksheet.Cells[row, col++].Value = orderDetail.ProductId;
                        worksheet.Cells[row, col++].Value = orderDetail.Product_Name_Ar;
                        worksheet.Cells[row, col++].Value = orderDetail.Product_Name_En;
                        worksheet.Cells[row, col++].Value = orderDetail.Unit_Price;
                        worksheet.Cells[row, col++].Value = orderDetail.Offer;
                        worksheet.Cells[row, col++].Value = orderDetail.Quantity_Of_Unit;
                    }

                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the package to a byte array
                var fileBytes = package.GetAsByteArray();
                return (true, null, fileBytes);
            }
        }

        // Helper methods
        private (DateTime? startDate, DateTime? endDate) GetDateRange(int? year, int? month)
        {
            if (!year.HasValue) return (null, null);

            if (month.HasValue)
                return (new DateTime(year.Value, month.Value, 1), new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value)));

            return (new DateTime(year.Value, 1, 1), new DateTime(year.Value, 12, 31));
        }

        private async Task<int> GetFilteredUserCountAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Users.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(u => u.Created_At >= startDate && u.Created_At <= endDate);

            return await query.CountAsync();
        }

        private async Task<decimal> GetFilteredOrderSumAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders.Where(o => o.Status_En == "Completed");

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            return await query.SumAsync(o => o.Total_Amount);
        }

        private async Task<int> GetOrderCountByStatusAsync(string status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders.Where(o => o.Status_En == status);

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            return await query.CountAsync();
        }
    }
}