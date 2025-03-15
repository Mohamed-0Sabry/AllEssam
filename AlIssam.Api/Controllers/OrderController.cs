using Microsoft.AspNetCore.Mvc;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Dtos.Request;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AlIssam.API.Extension;
using AlIssam.API.Services;

namespace AlIssam.API.Controllers
{
    /// <summary>
    /// Handles order processing and management
    /// </summary>
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get order history for current user
        /// </summary>
        /// <param name="page">Page number (default 1)</param>
        /// <returns>Paginated list of user's orders</returns>
        /// <response code="200">Returns order list</response>
        /// <response code="401">Unauthorized access</response>
        [HttpGet("api/order/{id}")]
        public async Task<IActionResult> GetUserOrders(int page = 1)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorResponse().CreateErrorResponse("معرف المستخدم مطلوب", "User ID is required."));

            var isAdmin = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value == "admin";
            if (!isAdmin && string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse().CreateErrorResponse("غير مصرح بالوصول", "Unauthroized."));

            var result = await _orderService.GetAllOrdersAsync(null,
              isAdmin ? null : userId,
              page,
              isAdmin ? null : userId);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="orderDto">Order details and items</param>
        /// <returns>Payment URL or order confirmation</returns>
        /// <response code="201">Order created successfully</response>
        /// <response code="400">Invalid order data</response>
        /// <response code="401">Unauthorized access</response>
        /// 
        /// TODO: adding jwt token for auth 
        [HttpPost("api/order/create")]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest orderDto)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorResponse().CreateErrorResponse("معرف المستخدم مطلوب", "User ID is required."));

            var result = await _orderService.CreateOrderAsync(orderDto, userId);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        /// <param name="id">Order ID to update</param>
        /// <param name="request">New status details</param>
        /// <returns>Updated order details</returns>
        /// <response code="200">Status updated successfully</response>
        /// <response code="400">Invalid status value</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">Order not found</response>
        [HttpPost("api/order/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, request);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        /// <param name="status">Filter by order status</param>
        /// <param name="userName">Filter by customer name</param>
        /// <param name="page">Page number (default 1)</param>
        /// <returns>Paginated list of all orders</returns>
        /// <response code="200">Returns order list</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpGet("api/orders")]
        [Authorize]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] string? status = null,
            [FromQuery] string? userName = null,
            [FromQuery] int page = 1)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new ErrorResponse().CreateErrorResponse("معرف المستخدم مطلوب", "User ID is required."));

            var isAdmin = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value == "admin";
            if (!isAdmin && string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse().CreateErrorResponse("غير مصرح بالوصول", "Unauthroized."));

            var result = await _orderService.GetAllOrdersAsync(status,
              isAdmin ? userName : null,
              page,
              isAdmin ? null : userId);
            return HandleServiceResult(result);
        }

        // Helper method to handle service results
        private IActionResult HandleServiceResult<T>(
            (bool isSuccess, ErrorResponse? errorResponse, T? response) result)
        {
            if (!result.isSuccess || result.errorResponse != null)
                return BadRequest(result.errorResponse);

            return Ok(result.response);
        }


        //[HttpGet("api/order/{id}")]
        //public async Task<IActionResult> GetOrderById(int id)
        //{
        //    // Query the database to get the order with its details
        //    var order = await _context.Orders
        //        .Include(o => o.User) // Include user details
        //        .Include(o => o.OrdersDetails) // Include order details
        //            .ThenInclude(od => od.Product) // Include product details
        //                .ThenInclude(p => p.ProductsImages) // Include product images
        //        .Include(o => o.OrdersDetails) // Include order details again
        //            .ThenInclude(od => od.ProductOptions) // Include product options
        //        .Include(o => o.Discount) // Include discount details (if applicable)
        //        .FirstOrDefaultAsync(o => o.Id == id);

        //    if (order == null)
        //    {
        //        return NotFound("Order not found.");
        //    }

        //    // Calculate the total amount
        //    decimal totalAmount = order.OrdersDetails.Sum(od => od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit);

        //    // Map the order to a DTO
        //    var orderDto = new GetOrderDataResponse
        //    {
        //        Id = order.Id,
        //        Email = order.User.Email,
        //        User_Name = order.User.UserName!,
        //        User_PhoneNumber = order.User.PhoneNumber!, // Include user phone number
        //        Status_En = order.Status_En,
        //        Status_Ar = order.Status_Ar,
        //        Order_Date = order.OrderDate,
        //        Total_Amount =  totalAmount,
        //        Payment_Method = "",
        //        // Include the total amount
        //        OrderDetails = order.OrdersDetails.Select(od => new OrderDetailsResponse
        //        {
        //            Product_Option_Id = od.ProductOptions.Id, // Include ProductOptions ID
        //            Name_Ar = od.Product.Unit_Of_Measurement_Ar, // Include ProductOptions name in Arabic
        //            Name_En = od.Product.Unit_Of_Measurement_En, // Include ProductOptions name in English
        //            ProductId = od.Product.Id,
        //            Image = od.Product.ProductsImages.FirstOrDefault(pi => pi.IsCover)!.Path, // Get the first image path (if available)
        //            Unit_Price = od.Unit_Price,
        //            Offer = od.Offer,
        //            Quantity_Of_Unit = od.Quantity_Of_Unit,
        //            Subtotal = od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit // Subtotal for this product
        //        }).ToList()
        //    };

        //    return Ok(orderDto);
        //}

        //[HttpGet("/api/orders/search")]
        //public async Task<IActionResult> SearchOrdersByStatus([FromQuery] string status)
        //{
        //    if (string.IsNullOrEmpty(status))
        //    {
        //        return BadRequest("Status query is required.");
        //    }

        //    // Search for orders by Status_Ar or Status_En
        //    var orders = await _context.Orders
        //        .Include(o => o.User) // Include user details
        //        .Include(o => o.OrdersDetails) // Include order details
        //            .ThenInclude(od => od.Product) // Include product details
        //        .Where(o => o.Status_Ar.Contains(status) || o.Status_En.Contains(status))
        //        .Select(o => new
        //        {
        //            Id = o.Id,
        //            User_Name = o.User.UserName,
        //            Status_En = o.Status_En,
        //            Status_Ar = o.Status_Ar,
        //            Order_Date = o.OrderDate,
        //            OrderDetails = o.OrdersDetails.Select(od => new
        //            {
        //                Product_Name_Ar = od.Product.Name_Ar,
        //                Product_Name_En = od.Product.Name_En,
        //                Price = od.Unit_Price,
        //                Offer = od.Offer,
        //                Quantity_Of_Unit = od.Quantity_Of_Unit,
        //                Product_Id = od.Product.Id,
        //            }).ToList()
        //        })
        //        .ToListAsync();

        //    if (orders == null || !orders.Any())
        //    {
        //        return NotFound("No orders found matching the status criteria.");
        //    }

        //    return Ok(orders);
        //}

        //  [HttpPost("api/order")]
        //  [Authorize]
        //  public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest orderCreateDto)
        //  {
        //      var nameIdentifier = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        //      // Validate the input
        //      if (nameIdentifier == null)
        //          return BadRequest("User ID not found.");

        //      // Find the user
        //      var user = await _context.Users.FindAsync(nameIdentifier);
        //      if (user == null)
        //          return NotFound("User not found.");

        //      // Check if the discount is valid (if provided)
        //      Discount discount = null;
        //      if (!string.IsNullOrEmpty(orderCreateDto.Code))
        //      {
        //          discount = await _context.Discounts
        //                      .FirstOrDefaultAsync(d =>
        //                      d.Code == orderCreateDto.Code && d.Status && d.StarDate <= DateTime.Now && d.EndDate >= DateTime.Now);

        //          if (discount == null)
        //              return BadRequest("Invalid or expired discount.");
        //      }

        //      // Create a new order
        //      var order = new Order
        //      {
        //          OrderDate = DateTime.Now,
        //          Status_En = "Pending", // Default status
        //          Status_Ar = "قيد الانتظار", // Default status in Arabic
        //          UserId = nameIdentifier,
        //          User = user,
        //           // Set DiscountId (nullable)
        //          OrdersDetails = new List<OrderDetails>() // Initialize the order details list
        //      };
        //      if (discount is not null){
        //          order.Discount = discount;
        //          order.DiscountId = discount.DiscountId;
        //      }

        //      // Calculate total amount and add order details
        //      decimal totalAmount = 0;
        //      foreach (var detailDto in orderCreateDto.Order_Details)
        //      {
        //          // Find the product
        //          var product = await _context.Products.FindAsync(detailDto.Product_Id);
        //          if (product == null)
        //          {
        //              return NotFound($"Product with ID {detailDto.Product_Id} not found.");
        //          }

        //          // Find the product option
        //          var productOption = await _context.ProductsOptions
        //              .FirstOrDefaultAsync(po => po.Id == detailDto.Product_Option_Id && po.ProductId == detailDto.Product_Id);
        //          if (productOption == null)
        //          {
        //              return NotFound($"Product option with ID {detailDto.Product_Option_Id} for product ID {detailDto.Product_Id} not found.");
        //          }

        //          // Create order detail using price and offer from the product option
        //          var orderDetail = new OrderDetails
        //          {
        //              ProductId = detailDto.Product_Id,
        //              ProductOptionsId = detailDto.Product_Option_Id, // Save ProductOptionsId
        //              Product = product,
        //              ProductOptions = productOption, // Set navigation property
        //              Unit_Price = productOption.Price, // Use price from ProductOption
        //              Offer = productOption.Offer, // Use offer from ProductOption
        //              Quantity_Of_Unit = detailDto.Quantity
        //          };

        //          // Add to order
        //          order.OrdersDetails.Add(orderDetail);

        //          // Calculate subtotal for this product
        //          decimal subtotal = productOption.Offer ?? productOption.Price * detailDto.Quantity;
        //          totalAmount += subtotal;
        //      }

        //      // Set total amount
        //      order.Total_Amount = (int)totalAmount;

        //      // Apply discount if valid
        //      if (discount != null)
        //      {
        //          decimal discountAmount = totalAmount * (discount.Percent / 100m);
        //          order.Total_Amount_With_Discount = (int)(totalAmount - discountAmount);
        //      }
        //      else
        //      {
        //          order.Total_Amount_With_Discount = order.Total_Amount; // No discount applied
        //      }

        //      // Save the order to the database
        //      _context.Orders.Add(order);
        //      await _context.SaveChangesAsync();

        //      // Map the created order to a DTO
        //      var orderDto = new 
        //      {
        //          Id = order.Id,
        //          User_Name = order.User.UserName,
        //          Status_En = order.Status_En,
        //          Status_Ar = order.Status_Ar,
        //          Order_Date = order.OrderDate,
        //          Total_Amount = order.Total_Amount,
        //          Total_Amount_With_Discount = order.Total_Amount_With_Discount,
        //          Discount_Id = order.DiscountId,
        //          OrderDetails = order.OrdersDetails.Select(od => new
        //          {
        //              Product_Name_Ar = od.Product.Name_Ar,
        //              Product_Name_En = od.Product.Name_En,
        //              Price = od.Unit_Price,
        //              Offer = od.Offer,
        //              Quantity_Of_Unit = od.Quantity_Of_Unit,
        //              Product_Id = od.Product.Id,
        //              Product_Option_Id = od.ProductOptionsId // Include ProductOptionsId in the response
        //          }).ToList()
        //      };

        //      return Ok(orderDto);
        //  }

        //  [HttpPut("api/order/{id}")]
        //  //[Authorize]
        //  public async Task<IActionResult> UpdateOrderStatus(
        //          int id,
        //          [FromBody] UpdateStatusRequest request) // Status is passed as a query parameter
        //      {
        //          // Validate the input
        //          if (string.IsNullOrEmpty(request.Status))
        //          {
        //              return BadRequest("Status is required.");
        //          }

        //          // Find the order
        //          var order = await _context.Orders.Include(o => o.User).Where(o => o.Id == id).FirstOrDefaultAsync();
        //          if (order == null)
        //          {
        //              return NotFound("Order not found.");
        //          }

        //          // Map the English status to the corresponding Arabic status
        //          string statusEn = request.Status.Trim();
        //          string statusAr;

        //          switch (statusEn.ToLower())
        //          {
        //              case "process":
        //                  statusEn = "Process";
        //                  statusAr = "قيد المعالجة";
        //                  break;
        //              case "completed":
        //                  statusEn = "Completed";
        //                  statusAr = "مكتمل";
        //                  break;
        //              case "cancelled":
        //                  statusEn = "Cancelled";
        //                  statusAr = "ملغى";
        //                  break;
        //              default:
        //                  return BadRequest("Invalid status. Allowed values are: Processing, Completed, Cancelled.");
        //          }

        //          // Update the order status
        //          order.Status_En = statusEn;
        //          order.Status_Ar = statusAr;

        //          // Save changes to the database
        //          _context.Orders.Update(order);
        //          await _context.SaveChangesAsync();

        //          // Return the updated order
        //          var orderDto = new
        //          {
        //              Id = order.Id,
        //              Status_En = order.Status_En,
        //              Status_Ar = order.Status_Ar,
        //              Order_Date = order.OrderDate,
        //              User_Name = order.User.UserName,
        //              Total_Amount = order.Total_Amount,
        //              Total_Amount_With_Discount = order.Total_Amount_With_Discount
        //          };

        //          return Ok(orderDto);
        //      }

        //  [HttpGet("api/orders")]
        //  public async Task<IActionResult> GetAllOrders(
        //[FromQuery] string? status = null, // Optional status filter
        //[FromQuery] string? userName = null, // Optional user name filter
        //[FromQuery] int page = 1)         // Page number (default: 1)
        //  {
        //      // Hardcoded page size
        //      const int pageSize = 20; // Number of items per page

        //      // Base query
        //      var ordersQuery = _context.Orders
        //          .Include(o => o.User) // Include user details
        //          .Include(o => o.OrdersDetails) // Include order details
        //              .ThenInclude(od => od.Product) // Include product details directly from OrderDetails
        //                  .ThenInclude(p => p.ProductsImages) // Include product images
        //          .Include(o => o.OrdersDetails) // Include order details again
        //              .ThenInclude(od => od.ProductOptions) // Include product options directly from OrderDetails
        //          .Include(o => o.Discount) // Include discount details (if applicable)
        //          .AsQueryable();

        //      // Apply status filter if provided
        //      if (!string.IsNullOrEmpty(status))
        //      {
        //          ordersQuery = ordersQuery
        //              .Where(o => o.Status_En == status || o.Status_Ar == status);
        //      }

        //      // Apply user name filter if provided
        //      if (!string.IsNullOrEmpty(userName))
        //      {
        //          ordersQuery = ordersQuery
        //              .Where(o => o.User.UserName != null && o.User.UserName.Contains(userName));
        //      }

        //      // Get the total count of orders (for pagination)
        //      var totalOrders = await ordersQuery.CountAsync();

        //      // Apply pagination and map to DTO
        //      var orders = await ordersQuery
        //          .Select(o => new GetOrderDataResponse
        //          {
        //              Id = o.Id,
        //              User_Name = o.User.UserName!, // Assuming UserName is not null due to the filter
        //              Email = o.User.Email,
        //              User_PhoneNumber = o.User.PhoneNumber!, // Include user phone number
        //              Status_En = o.Status_En,
        //              Status_Ar = o.Status_Ar,
        //              Order_Date = o.OrderDate,
        //              Total_Amount = o.OrdersDetails.Sum(od => od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit), // Total amount for the order
        //              OrderDetails = o.OrdersDetails.Select(od => new OrderDetailsResponse
        //              {
        //                  Product_Option_Id = od.ProductOptions.Id,
        //                  Product_Name_Ar = od.Product.Name_Ar,
        //                  Product_Name_En = od.Product.Name_En,
        //                  Name_Ar = od.Product.Unit_Of_Measurement_Ar,
        //                  Name_En = od.Product.Unit_Of_Measurement_En,
        //                  ProductId = od.Product.Id,
        //                  Image = od.Product.ProductsImages.FirstOrDefault(x => x.IsCover)!.Path, // Get the first image path (if available)
        //                  Unit_Price = od.Unit_Price,
        //                  Offer = od.Offer,
        //                  Quantity_Of_Unit = od.Quantity_Of_Unit,
        //                  Subtotal = od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit // Subtotal for this product
        //              }).ToList()
        //          })
        //          .Skip((page - 1) * pageSize) // Skip items on previous pages
        //          .Take(pageSize)              // Take items for the current page
        //          .ToListAsync();

        //      // Create a paginated response
        //      var pagedOrders = PagedList<GetOrderDataResponse>.Create(orders, page, pageSize, totalOrders);

        //      return Ok(pagedOrders);
        //  }

    }
}