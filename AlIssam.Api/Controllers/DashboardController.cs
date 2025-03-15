using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Services;
using AlIssam.API.Services.interFaces;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.API.Controllers
{
    /// <summary>
    /// Provides business intelligence and reporting endpoints
    /// </summary>
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get key performance indicators
        /// </summary>
        /// <param name="year">Filter by year</param>
        /// <param name="month">Filter by month</param>
        /// <returns>Dashboard statistics</returns>
        /// <response code="200">Returns statistics data</response>
        [HttpGet("api/dashboard/statistics")]
        public async Task<IActionResult> GetDashboardStatistics(
            [FromQuery] int? year,  // Filter by year (e.g., 2023)
            [FromQuery] int? month) // Filter by month (e.g., 10 for October)
        {
            var result = await _dashboardService.GetDashboardStatisticsAsync(year, month);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// Export dashboard statistics to Excel
        /// </summary>
        /// <param name="year">Filter by year</param>
        /// <param name="month">Filter by month</param>
        /// <returns>Excel file with statistics</returns>
        /// <response code="200">Returns Excel file</response>
        [HttpGet("api/dashboard/statistics/excel-export")]
        public async Task<IActionResult> ExportDashboardStatisticsToExcel(
            [FromQuery] int? year,  // Filter by year (e.g., 2023)
            [FromQuery] int? month) // Filter by month (e.g., 10 for October)
        {
            var result = await _dashboardService.ExportDashboardStatisticsToExcelAsync(year, month);
            return HandleFileServiceResult(result, "DashboardStatistics.xlsx");
        }

        [HttpGet("api/dashboard/users/excel-export")]
        public async Task<IActionResult> ExportUsersToExcel()
        {
            var result = await _dashboardService.ExportUsersToExcelAsync();
            return HandleFileServiceResult(result, "Users.xlsx");
        }

        [HttpGet("api/dashboard/products/excel-export")]
        public async Task<IActionResult> ExportProductsToExcel()
        {
            var result = await _dashboardService.ExportProductsToExcelAsync();
            return HandleFileServiceResult(result, "Products.xlsx");
        }

        [HttpGet("api/dashboard/orders/excel-export")]
        public async Task<IActionResult> ExportOrdersToExcel()
        {
            var result = await _dashboardService.ExportOrdersToExcelAsync();
            return HandleFileServiceResult(result, "Orders.xlsx");
        }

        // Helper method to handle normal service responses
        private IActionResult HandleServiceResult<T>(
            (bool isSuccess, ErrorResponse? errorResponse, T? response) result)
        {
            if (!result.isSuccess || result.errorResponse != null)
                return BadRequest(result.errorResponse);

            return Ok(result.response);
        }

        // Helper method to handle file service responses
        private IActionResult HandleFileServiceResult(
            (bool isSuccess, ErrorResponse? errorResponse, byte[]? fileBytes) result, string fileName)
        {
            if (!result.isSuccess || result.errorResponse != null)
                return BadRequest(result.errorResponse);

            if (result.fileBytes == null)
                return NotFound("File content is missing.");

            return File(result.fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        //  [HttpGet("api/dashboard/statistics")]
        //  public async Task<IActionResult> GetDashboardStatistics(
        //    [FromQuery] int? year,  // Filter by year (e.g., 2023)
        //    [FromQuery] int? month) // Filter by month (e.g., 10 for October)
        //  {
        //      // Validate year and month inputs
        //      if (year.HasValue && (year < 1900 || year > DateTime.UtcNow.Year))
        //      {
        //          return BadRequest("Invalid year. Year must be between 1900 and the current year.");
        //      }

        //      if (month.HasValue && (month < 1 || month > 12))
        //      {
        //          return BadRequest("Invalid month. Month must be between 1 and 12.");
        //      }

        //      // Calculate start and end dates based on year and month
        //      DateTime? startDate = null;
        //      DateTime? endDate = null;

        //      if (year.HasValue)
        //      {
        //          if (month.HasValue)
        //          {
        //              // Filter by specific year and month
        //              startDate = new DateTime(year.Value, month.Value, 1);
        //              endDate = new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value));
        //          }
        //          else
        //          {
        //              // Filter by entire year
        //              startDate = new DateTime(year.Value, 1, 1);
        //              endDate = new DateTime(year.Value, 12, 31);
        //          }
        //      }

        //      // Get total users (filtered by date if applicable)
        //      var totalUsersQuery = _context.Users.AsQueryable();
        //      if (startDate.HasValue && endDate.HasValue)
        //      {
        //          totalUsersQuery = totalUsersQuery.Where(u => u.Created_At >= startDate && u.Created_At <= endDate);
        //      }
        //      var totalUsers = await totalUsersQuery.CountAsync();

        //      // Get completed orders (filtered by date if applicable)
        //      var completedOrdersQuery = _context.Orders.Where(o => o.Status_En == "Completed");
        //      if (startDate.HasValue && endDate.HasValue)    
        //          completedOrdersQuery = completedOrdersQuery.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

        //      var completedOrders = await completedOrdersQuery.CountAsync();

        //      // Get cancelled orders (filtered by date if applicable)
        //      var cancelledOrdersQuery = _context.Orders.Where(o => o.Status_En == "Cancelled");
        //      if (startDate.HasValue && endDate.HasValue)
        //          cancelledOrdersQuery = cancelledOrdersQuery.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

        //      var cancelledOrders = await cancelledOrdersQuery.CountAsync();

        //      // Calculate total payment and profit (example logic, adjust as needed)
        //      var totalPaymentQuery = _context.Orders.Where(o => o.Status_En == "Completed");
        //      if (startDate.HasValue && endDate.HasValue)
        //          totalPaymentQuery = totalPaymentQuery.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);


        //      var totalPayment = await totalPaymentQuery.SumAsync(o => o.Total_Amount);

        //      var profit = totalPayment * 0.2m; // Example: 20% profit

        //      // Prepare response
        //      var response = new DashboardStatisticsResponse
        //      {
        //          Total_Users = totalUsers,
        //          Completed_Orders = completedOrders,
        //          Cancelled_Orders = cancelledOrders,
        //          Total_Payment = totalPayment,
        //          Profit = profit,
        //      };

        //      return Ok(response);
        //  }


        //  [HttpGet("api/dashboard/statistics/excel-export")]
        //  public async Task<IActionResult> ExportDashboardStatisticsToExcel(
        //[FromQuery] int? year,  // Filter by year (e.g., 2023)
        //[FromQuery] int? month) // Filter by month (e.g., 10 for October)
        //  {
        //      // Validate year and month inputs
        //      if (year.HasValue && (year < 1900 || year > DateTime.UtcNow.Year))
        //      {
        //          return BadRequest("Invalid year. Year must be between 1900 and the current year.");
        //      }

        //      if (month.HasValue && (month < 1 || month > 12))
        //      {
        //          return BadRequest("Invalid month. Month must be between 1 and 12.");
        //      }

        //      // Calculate start and end dates based on year and month
        //      DateTime? startDate = null;
        //      DateTime? endDate = null;

        //      if (year.HasValue)
        //      {
        //          if (month.HasValue)
        //          {
        //              // Filter by specific year and month
        //              startDate = new DateTime(year.Value, month.Value, 1);
        //              endDate = new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value));
        //          }
        //          else
        //          {
        //              // Filter by entire year
        //              startDate = new DateTime(year.Value, 1, 1);
        //              endDate = new DateTime(year.Value, 12, 31);
        //          }
        //      }

        //      // Query the database to get statistics
        //      var totalUsers = await _context.Users
        //          .Where(u => startDate == null || (u.Created_At >= startDate && u.Created_At <= endDate))
        //          .CountAsync();

        //      var completedOrders = await _context.Orders
        //          .Where(o => o.Status_En == "Completed" && (startDate == null || (o.OrderDate >= startDate && o.OrderDate <= endDate)))
        //          .CountAsync();

        //      var cancelledOrders = await _context.Orders
        //          .Where(o => o.Status_En == "Cancelled" && (startDate == null || (o.OrderDate >= startDate && o.OrderDate <= endDate)))
        //          .CountAsync();

        //      var totalPayment = await _context.Orders
        //          .Where(o => o.Status_En == "Completed" && (startDate == null || (o.OrderDate >= startDate && o.OrderDate <= endDate)))
        //          .SumAsync(o => o.Total_Amount);

        //      var profit = totalPayment * 0.2m; // Example: 20% profit

        //      // Create an Excel package
        //      using (var package = new ExcelPackage())
        //      {
        //          // Add a worksheet
        //          var worksheet = package.Workbook.Worksheets.Add("Dashboard Statistics");

        //          // Add headers
        //          worksheet.Cells[1, 1].Value = "Metric";
        //          worksheet.Cells[1, 2].Value = "Value";

        //          // Add data
        //          worksheet.Cells[2, 1].Value = "Total Users";
        //          worksheet.Cells[2, 2].Value = totalUsers;

        //          worksheet.Cells[3, 1].Value = "Completed Orders";
        //          worksheet.Cells[3, 2].Value = completedOrders;

        //          worksheet.Cells[4, 1].Value = "Cancelled Orders";
        //          worksheet.Cells[4, 2].Value = cancelledOrders;

        //          worksheet.Cells[5, 1].Value = "Total Payment";
        //          worksheet.Cells[5, 2].Value = totalPayment;
        //          worksheet.Cells[5, 2].Style.Numberformat.Format = "$#,##0.00";

        //          worksheet.Cells[6, 1].Value = "Profit";
        //          worksheet.Cells[6, 2].Value = profit;
        //          worksheet.Cells[6, 2].Style.Numberformat.Format = "$#,##0.00";

        //          // Auto-fit columns for better readability
        //          worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        //          // Convert the package to a byte array
        //          var fileBytes = package.GetAsByteArray();

        //          // Return the Excel file as a downloadable file
        //          return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DashboardStatistics.xlsx");
        //      }
        //  }

        //  [HttpGet("api/dashboard/users/excel-export")]
        //  public async Task<IActionResult> ExportUsersToExcel()
        //  {
        //      // Query the database to get user data
        //      var users = await _context.Users
        //          .Select(u => new
        //          {
        //              u.UserName,
        //              u.Email,
        //              u.City,
        //              Total_Orders = u.Orders.Count // Assuming a navigation property `Orders` exists
        //          })
        //          .ToListAsync();

        //      // Create an Excel package
        //      using (var package = new ExcelPackage())
        //      {
        //          // Add a worksheet
        //          var worksheet = package.Workbook.Worksheets.Add("Users");

        //          // Add headers
        //          worksheet.Cells[1, 1].Value = "User Name";
        //          worksheet.Cells[1, 2].Value = "Email";
        //          worksheet.Cells[1, 3].Value = "Total Orders";
        //          worksheet.Cells[1, 4].Value = "City";


        //          // Add data
        //          for (int i = 0; i < users.Count; i++)
        //          {
        //              worksheet.Cells[i + 2, 1].Value = users[i].UserName;
        //              worksheet.Cells[i + 2, 2].Value = users[i].Email;
        //              worksheet.Cells[i + 2, 3].Value = users[i].Total_Orders;
        //              worksheet.Cells[i + 2, 4].Value = users[i].City;

        //          }

        //          // Auto-fit columns for better readability
        //          worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        //          // Convert the package to a byte array
        //          var fileBytes = package.GetAsByteArray();

        //          // Return the Excel file as a downloadable file
        //          return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
        //      }
        //  }

        //  [HttpGet("api/dashboard/products/excel-export")]
        //  public async Task<IActionResult> ExportProductsToExcel()
        //  {
        //      // Query the database to get product data including options
        //      var products = await _context.Products
        //          .Include(p => p.Quantity) // Include the ProductOptions navigation property
        //          .Select(p => new
        //          {
        //              p.Id,
        //              p.Name_Ar,
        //              p.Name_En,
        //              p.Description_Ar,
        //              p.Description_En,
        //              p.Slug_Ar,
        //              p.Slug_En,
        //              p.IsEnded,
        //              Total_Images = p.ProductsImages.Count,
        //              Options = p.Quantity.Select(q => new 
        //              {
        //                  q.Id,
        //                  Name_Ar  = p.Unit_Of_Measurement_Ar,
        //                  Name_En = p.Unit_Of_Measurement_En,
        //                  p.Unit_Of_Measurement_Ar,
        //                  q.Price,
        //                  q.Offer,
        //                  q.Default
        //              }).ToList()
        //          })
        //          .ToListAsync();

        //      // Create an Excel package
        //      using (var package = new ExcelPackage())
        //      {
        //          // Add a worksheet
        //          var worksheet = package.Workbook.Worksheets.Add("Products");

        //          // Add headers for product details
        //          worksheet.Cells[1, 1].Value = "ID";
        //          worksheet.Cells[1, 2].Value = "Name (Arabic)";
        //          worksheet.Cells[1, 3].Value = "Name (English)";
        //          worksheet.Cells[1, 4].Value = "Description (Arabic)";
        //          worksheet.Cells[1, 5].Value = "Description (English)";
        //          worksheet.Cells[1, 6].Value = "Slug (Arabic)";
        //          worksheet.Cells[1, 7].Value = "Slug (English)";
        //          worksheet.Cells[1, 8].Value = "Is Ended";
        //          worksheet.Cells[1, 9].Value = "Total Images";

        //          // Determine the maximum number of options for any product
        //          int maxOptions = products.Max(p => p.Options.Count);

        //          // Add headers for options dynamically
        //          int col = 10; // Start column for options
        //          for (int i = 1; i <= maxOptions; i++)
        //          {
        //              worksheet.Cells[1, col++].Value = $"Option{i}_Id";
        //              worksheet.Cells[1, col++].Value = $"Option{i}_Name_Ar";
        //              worksheet.Cells[1, col++].Value = $"Option{i}_Name_En";
        //              worksheet.Cells[1, col++].Value = $"Option{i}_Price";
        //              worksheet.Cells[1, col++].Value = $"Option{i}_Offer";
        //              worksheet.Cells[1, col++].Value = $"Option{i}_Default";
        //          }

        //          // Add data
        //          int row = 2; // Start from row 2 (row 1 is headers)
        //          foreach (var product in products)
        //          {
        //              // Write product details
        //              worksheet.Cells[row, 1].Value = product.Id;
        //              worksheet.Cells[row, 2].Value = product.Name_Ar;
        //              worksheet.Cells[row, 3].Value = product.Name_En;
        //              worksheet.Cells[row, 4].Value = product.Description_Ar;
        //              worksheet.Cells[row, 5].Value = product.Description_En;
        //              worksheet.Cells[row, 6].Value = product.Slug_Ar;
        //              worksheet.Cells[row, 7].Value = product.Slug_En;
        //              worksheet.Cells[row, 8].Value = product.IsEnded;
        //              worksheet.Cells[row, 9].Value = product.Total_Images;

        //              // Write options dynamically
        //              col = 10; // Reset column for options
        //              foreach (var option in product.Options)
        //              {
        //                  worksheet.Cells[row, col++].Value = option.Id;
        //                  worksheet.Cells[row, col++].Value = option.Name_Ar;
        //                  worksheet.Cells[row, col++].Value = option.Name_En;
        //                  worksheet.Cells[row, col++].Value = option.Price;
        //                  worksheet.Cells[row, col++].Value = option.Offer;
        //                  worksheet.Cells[row, col++].Value = option.Default;
        //              }

        //              row++; // Move to the next row for the next product
        //          }

        //          // Auto-fit columns for better readability
        //          worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        //          // Convert the package to a byte array
        //          var fileBytes = package.GetAsByteArray();

        //          // Return the Excel file as a downloadable file
        //          return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
        //      }
        //  }

        //  [HttpGet("api/dashboard/orders/excel-export")]
        //  public async Task<IActionResult> ExportOrdersToExcel()
        //  {
        //      // Query the database to get order data including order details
        //      var orders = await _context.Orders
        //          .Include(o => o.User) // Include user details
        //          .Include(o => o.OrdersDetails) // Include order details
        //              .ThenInclude(od => od.Product) // Include product details
        //          .Select(o => new
        //          {
        //              o.Id,
        //              o.OrderDate,
        //              o.Status_En,
        //              o.Status_Ar,
        //              User_Name = o.User.UserName,
        //              OrderDetails = o.OrdersDetails.Select(od => new
        //              {
        //                  od.Id,
        //                  od.ProductId,
        //                  od.Unit_Price,
        //                  od.Offer,
        //                  od.Quantity_Of_Unit,
        //                  Product_Name_Ar = od.Product.Name_Ar,
        //                  Product_Name_En = od.Product.Name_En
        //              }).ToList()
        //          })
        //          .ToListAsync();

        //      // Create an Excel package
        //      using (var package = new ExcelPackage())
        //      {
        //          // Add a worksheet
        //          var worksheet = package.Workbook.Worksheets.Add("Orders");

        //          // Add headers for order details
        //          worksheet.Cells[1, 1].Value = "Order ID";
        //          worksheet.Cells[1, 2].Value = "Order Date";
        //          worksheet.Cells[1, 3].Value = "Status (English)";
        //          worksheet.Cells[1, 4].Value = "Status (Arabic)";
        //          worksheet.Cells[1, 5].Value = "User Name";

        //          // Determine the maximum number of order details for any order
        //          int maxOrderDetails = orders.Max(o => o.OrderDetails.Count);

        //          // Add headers for order details dynamically
        //          int col = 6; // Start column for order details
        //          for (int i = 1; i <= maxOrderDetails; i++)
        //          {
        //              worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Id";
        //              worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Product_Id";
        //              worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Product_Name_Ar";
        //              worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Product_Name_En";
        //              worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Unit_Price";
        //              worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Offer";
        //              worksheet.Cells[1, col++].Value = $"OrderDetail{i}_Quantity_Of_Unit";
        //          }

        //          // Add data
        //          int row = 2; // Start from row 2 (row 1 is headers)
        //          foreach (var order in orders)
        //          {
        //              // Write order details
        //              worksheet.Cells[row, 1].Value = order.Id;
        //              worksheet.Cells[row, 2].Value = order.OrderDate;
        //              worksheet.Cells[row, 3].Value = order.Status_En;
        //              worksheet.Cells[row, 4].Value = order.Status_Ar;
        //              worksheet.Cells[row, 5].Value = order.User_Name;

        //              // Write order details dynamically
        //              col = 6; // Reset column for order details
        //              foreach (var orderDetail in order.OrderDetails)
        //              {
        //                  worksheet.Cells[row, col++].Value = orderDetail.Id;
        //                  worksheet.Cells[row, col++].Value = orderDetail.ProductId;
        //                  worksheet.Cells[row, col++].Value = orderDetail.Product_Name_Ar;
        //                  worksheet.Cells[row, col++].Value = orderDetail.Product_Name_En;
        //                  worksheet.Cells[row, col++].Value = orderDetail.Unit_Price;
        //                  worksheet.Cells[row, col++].Value = orderDetail.Offer;
        //                  worksheet.Cells[row, col++].Value = orderDetail.Quantity_Of_Unit;
        //              }

        //              row++; // Move to the next row for the next order
        //          }

        //          // Auto-fit columns for better readability
        //          worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        //          // Convert the package to a byte array
        //          var fileBytes = package.GetAsByteArray();

        //          // Return the Excel file as a downloadable file
        //          return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
        //      }
        //  }

    }
}
