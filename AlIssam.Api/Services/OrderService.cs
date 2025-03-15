using Microsoft.EntityFrameworkCore;
using AlIssam.DataAccessLayer.Entities;
using AlIssam.API.Dtos.Response;
using AlIssam.DataAccessLayer;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos;
using AlIssam.API.Extension;
using System.Linq.Expressions;
using AlIssam.Api.Services;
using AlIssam.Api.Services.interFaces;

namespace AlIssam.API.Services
{
    /// <summary>
    /// Manages order processing, payment integration, and order history
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly AlIssamDbContext _context;
        private readonly IFatoorahService _fatoorahService;

        public OrderService(AlIssamDbContext context, IFatoorahService fatoorahService)
        {
            _context = context;
            _fatoorahService = fatoorahService;
        }
        private async Task<string> GenerateUniqueOrderSerial()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string serial;
            bool exists;
            do
            {
                serial = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                exists = await _context.Orders.AnyAsync(o => o.Id == serial);
            } while (exists);

            return serial;
        }
        // Private static selector for mapping Order to GetOrderDataResponse
        private static readonly Expression<Func<Order, GetOrderDataResponse>> OrderSelector = order => new GetOrderDataResponse
        {
            Id = order.Id,
            Email = order.User.Email,
            User_Name = order.User.UserName!,
            User_PhoneNumber = order.User.PhoneNumber!,
            Receiver_Number = order.ReceiverDetails.Phone,
            Status_En = order.Status_En,
            Status_Ar = order.Status_Ar,
            IsPaid = order.Payment_Status_En == "Paid" || order.Payment_Status_En == "Cash on Delivery",
            Order_Date = order.OrderDate,
            Total_Amount = order.OrdersDetails.Sum(od => od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit),
            Payment_Method = order.Payment_Method,
            OrderDetails = order.OrdersDetails.Select(od => new OrderDetailsResponse
            {
                Product_Name_Ar = od.Product.Name_Ar,
                Product_Name_En = od.Product.Name_En,
                Product_Option_Id = od.ProductOptions.Id,
                Name_Ar = od.Product.Unit_Of_Measurement_Ar,
                Name_En = od.Product.Unit_Of_Measurement_En,
                ProductId = od.Product.Id,
                Image = od.Product.ProductsImages.FirstOrDefault(pi => pi.IsCover)!.Path,
                Unit_Price = od.Unit_Price,
                Offer = od.ProductOptions.Offer,
                Quantity_Of_Unit = od.Quantity_Of_Unit,
                Subtotal = od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit
            }).ToList()
        };

        private static readonly Expression<Func<Order, GetOrderDataResponse>> GetAllOrderSelector = order => new GetOrderDataResponse
        {
            Id = order.Id,
            Email = order.User.Email,
            User_Name = order.User.UserName!,
            User_PhoneNumber = order.ReceiverDetails.Phone,
            Status_En = order.Status_En,
            Status_Ar = order.Status_Ar,
            Order_Date = order.OrderDate,
            IsPaid = order.Payment_Status_En == "Paid",
            Total_Amount = order.OrdersDetails.Sum(od => od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit),
            Payment_Method = order.Payment_Method,
            OrderDetails = order.OrdersDetails.Select(od => new OrderDetailsResponse
            {
                Product_Name_Ar = od.Product.Name_Ar,
                Product_Name_En = od.Product.Name_En,
                Product_Option_Id = od.ProductOptions.Id,
                Name_Ar = od.Product.Unit_Of_Measurement_Ar,
                Name_En = od.Product.Unit_Of_Measurement_En,
                ProductId = od.Product.Id,
                Image = od.Product.ProductsImages.FirstOrDefault(pi => pi.IsCover)!.Path,
                Unit_Price = od.Unit_Price,
                Offer = od.ProductOptions.Offer,
                Quantity_Of_Unit = od.Quantity_Of_Unit,
                Subtotal = od.Offer ?? od.Unit_Price * od.Quantity_Of_Unit
            }).ToList()
        };

        private static (string Ar, string En) GetPaymentStatus(string paymentMethod)
        {
            return paymentMethod?.ToLower() switch
            {
                "cash" => ("كاش عند التسليم", "Cash on Delivery"),
                "payment" => ("غير مدفوع", "unpaid"),
                _ => ("غير مدفوع", "unpaid") 
            };
        }

        private async Task<int> GenerateUniqueInvoiceId()
        {
            var random = new Random();
            int invoiceId;
            bool exists;

            do
            {
                // Generate a 7-digit number (1000000 to 9999999)
                invoiceId = random.Next(1000000, 10000000);

                // Check if the generated invoice ID already exists in the database
                exists = await _context.Orders.AnyAsync(o => o.InvoiceId == invoiceId);
            } while (exists);

            return invoiceId;
        }

        public async Task<(bool isSuccess, ErrorResponse? errorMessage, GetOrderDataResponse? result)> GetOrderByIdAsync(string id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ReceiverDetails)
                .Include(o => o.OrdersDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductsImages)
                .Include(o => o.OrdersDetails)
                    .ThenInclude(od => od.ProductOptions)
                .Include(o => o.Discount)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return (false, new ErrorResponse().CreateErrorResponse("الطلب غير موجود", "Order not found."), null);
            }

            // Compile and execute the expression
            var orderDto = OrderSelector.Compile()(order);
            return (true, null, orderDto);
        }

        /// <summary>
        /// Creates a new order and processes payment integration
        /// </summary>
        /// <param name="orderCreateDto">Order details and items</param>
        /// <param name="userId">Authenticated user ID</param>
        /// <returns>Tuple containing payment URL or error details</returns>
        /// <exception cref="DbUpdateException">Thrown for database errors during order creation</exception>
        public async Task<(bool isSuccess, ErrorResponse? errorMessage, string? paymentUrl)> CreateOrderAsync(CreateOrderRequest orderCreateDto, string userId)
        {
            // Start a new transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            {

                try
                {
                    if (string.IsNullOrEmpty(userId))
                        return (false, new ErrorResponse().CreateErrorResponse("معرف المستخدم مطلوب", "User ID is required."), null);

                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                        return (false, new ErrorResponse().CreateErrorResponse("المستخدم غير موجود", "User not found."), null);

                    Discount discount = null;
                    if (!string.IsNullOrEmpty(orderCreateDto.Code))
                    {
                        discount = await _context.Discounts
                            .FirstOrDefaultAsync(d => d.Code == orderCreateDto.Code && d.Status && d.StarDate <= DateTime.Now && d.EndDate >= DateTime.Now);

                        if (discount == null)
                            return (false, new ErrorResponse().CreateErrorResponse("الخصم غير صالح أو منتهي الصلاحية", "Invalid or expired discount."), null);
                    }

                    var order = new Order
                    {
                        Id = await GenerateUniqueOrderSerial(),
                        OrderDate = DateTime.Now,
                        Payment_Status_Ar = GetPaymentStatus(orderCreateDto.Payment_method).Ar,
                        Payment_Status_En = GetPaymentStatus(orderCreateDto.Payment_method).En,
                        Payment_Method = orderCreateDto.Payment_method.Equals("cash") ? "كاش عند التسليم" : "دفع إلكتروني",
                        Status_En = "Process",
                        Status_Ar = "قيد الانتظار",
                        UserId = userId,
                        User = user,
                        OrdersDetails = new List<OrderDetails>(),
                        ReceiverDetails = new OrderReceiverDetails
                        {
                            Location = orderCreateDto.Location,
                            Recevier = orderCreateDto.Recevier,
                            Phone = orderCreateDto.Phone
                        }
                    };

                    if (discount != null)
                    {
                        order.Discount = discount;
                        order.DiscountId = discount.DiscountId;
                    }

                    decimal totalAmount = 0;
                    foreach (var detailDto in orderCreateDto.Order_Details)
                    {
                        var product = await _context.Products.FindAsync(detailDto.Product_Id);
                        if (product == null)
                            return (false, new ErrorResponse().CreateErrorResponse($"المنتج بالمعرف {detailDto.Product_Id} غير موجود", $"Product with ID {detailDto.Product_Id} not found."), null);

                        var productOption = await _context.ProductsOptions
                            .FirstOrDefaultAsync(po => po.Id == detailDto.Product_Option_Id && po.ProductId == detailDto.Product_Id);
                        if (productOption == null)
                            return (false, new ErrorResponse().CreateErrorResponse($"خيار المنتج بالمعرف {detailDto.Product_Option_Id} غير موجود", $"Product option with ID {detailDto.Product_Option_Id} not found."), null);

                        var orderDetail = new OrderDetails
                        {
                            ProductId = detailDto.Product_Id,
                            ProductOptionsId = detailDto.Product_Option_Id,
                            Product = product,
                            ProductOptions = productOption,
                            Unit_Price = productOption.Price,
                            Offer = productOption.Offer,
                            Quantity_Of_Unit = detailDto.Quantity
                        };

                        order.OrdersDetails.Add(orderDetail);
                        decimal subtotal = productOption.Offer ?? productOption.Price * detailDto.Quantity;
                        totalAmount += subtotal;
                    }

                    order.Total_Amount = totalAmount;

                    if (discount != null)
                    {
                        decimal discountAmount = totalAmount * (discount.Percent / 100m);
                        order.Total_Amount_With_Discount = totalAmount - discountAmount;
                    }
                    else
                        order.Total_Amount_With_Discount = order.Total_Amount;

                    if (orderCreateDto.Payment_method.Equals("cash"))
                    {
                        order.InvoiceId = await GenerateUniqueInvoiceId();
                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return (true, null, "Created");
                    }
                    await _context.SaveChangesAsync();

                    var paymentRequest = new PaymentRequest
                    {
                        CustomerName = orderCreateDto.Recevier,
                        InvoiceValue = order.Total_Amount_With_Discount ?? order.Total_Amount,
                        DisplayCurrencyIso = "KWD",
                        NotificationOption = "LNK",
                        CallBackUrl = "https://alessam.store/ar/cart?success_pay=true",
                        ErrorUrl = "https://alessam.store/ar/cart?success_pay=false"
                    };

                    var response = await _fatoorahService.SendPaymentAsync(paymentRequest);

                    if (response.IsSuccess)
                    {
                        //response.Data.InvoiceId
                        Console.WriteLine(response.Data.InvoiceURL);
                        Console.WriteLine(response.Data.InvoiceId);

                        Console.WriteLine($"Payment sent with id ${response.Data.InvoiceId}");
                        order.InvoiceId = response.Data.InvoiceId;
                        //await _context.SaveChangesAsync();
                    }
                    _context.Orders.Add(order);

                    await _context.SaveChangesAsync();
                    order.OrderReceiverDetailsId = order.ReceiverDetails.OrderReceiverDetailsId;
                    _context.Orders.Update(order);
                    // Commit the transaction if everything succeeds
                    await transaction.CommitAsync();

                    return (true, null, response.Data.InvoiceURL);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();
                    return (false, new ErrorResponse().CreateErrorResponse("حدث خطأ أثناء إنشاء الطلب", $"An error occurred while creating the order: {ex.Message}"), null);
                }
            }
        }

        /// <summary>
        /// Updates order status (Process/Completed/Cancelled)
        /// </summary>
        /// <param name="id">Order ID to update</param>
        /// <param name="request">New status details</param>
        /// <returns>Tuple with updated order details or errors</returns>
        public async Task<(bool isSuccess, ErrorResponse? errorMessage, UpdateStatusResponse? result)> UpdateOrderStatusAsync(string id, UpdateStatusRequest request)
        {
            // Start a new transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrEmpty(request.Status))
                {
                    return (false, new ErrorResponse().CreateErrorResponse("الحالة مطلوبة", "Status is required."), null);
                }

                var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                    return (false, new ErrorResponse().CreateErrorResponse("الطلب غير موجود", "Order not found."), null);
                }

                string statusEn = request.Status.Trim();
                string statusAr;

                switch (statusEn.ToLower())
                {
                    case "process":
                        statusEn = "Process";
                        statusAr = "قيد المعالجة";
                        break;
                    case "completed":
                        statusEn = "Completed";
                        statusAr = "مكتمل";
                        break;
                    case "cancelled":
                        statusEn = "Cancelled";
                        statusAr = "ملغى";
                        break;
                    default:
                        return (false, new ErrorResponse().CreateErrorResponse("حالة غير صالحة", "Invalid status."), null);
                }

                order.Status_En = statusEn;
                order.Status_Ar = statusAr;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // Commit the transaction if everything succeeds
                await transaction.CommitAsync();

                var orderDto = new UpdateStatusResponse
                {
                    Id = order.Id,
                    Status_En = order.Status_En,
                    Status_Ar = order.Status_Ar,
                    Order_Date = order.OrderDate,
                    User_Name = order.User.UserName!,
                    Total_Amount = order.Total_Amount,
                    Total_Amount_With_Discount = order.Total_Amount_With_Discount
                };

                return (true, null, orderDto);
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                return (false, new ErrorResponse().CreateErrorResponse("حدث خطأ أثناء تحديث حالة الطلب", $"An error occurred while updating the order status: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Retrieves paginated order history with filtering options
        /// </summary>
        /// <param name="status">Filter by order status</param>
        /// <param name="userName">Filter by customer name</param>
        /// <param name="page">Pagination page number</param>
        /// <param name="userId">Optional user ID filter</param>
        /// <returns>Paged list of order details</returns>
        public async Task<(bool isSuccess, ErrorResponse? errorMessage, PagedList<GetOrderDataResponse>? result)> GetAllOrdersAsync(string? status, string? userName, int page, string? userId = null)
        {
            const int pageSize = 20;

            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.ReceiverDetails)
                .Include(o => o.OrdersDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductsImages)
                .Include(o => o.OrdersDetails)
                    .ThenInclude(od => od.ProductOptions)
                .Include(o => o.Discount)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                ordersQuery = ordersQuery.Where(o => o.UserId == userId);
            }
            else
            {
                // Apply admin-only filters
                if (!string.IsNullOrEmpty(status))
                {
                    ordersQuery = ordersQuery
                        .Where(o => o.Status_En == status || o.Status_Ar == status);
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    ordersQuery = ordersQuery
                        .Where(o => o.User.UserName != null && o.User.UserName.Contains(userName));
                }
            }

            var totalOrders = await ordersQuery.CountAsync();

            var orders = await ordersQuery
                .Select(GetAllOrderSelector)
                .OrderByDescending(o => o.Order_Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedOrders = PagedList<GetOrderDataResponse>.Create(orders, page, pageSize, totalOrders);

            return (true, null, pagedOrders);
        }
    }
}