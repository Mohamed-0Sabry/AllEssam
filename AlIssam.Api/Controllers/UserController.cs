using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlIssam.API.Dtos;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.API.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AlIssamDbContext _context;

        public UserController(UserManager<User> userManager, AlIssamDbContext AlIssamDbContext)
        {
            _userManager = userManager;
            _context = AlIssamDbContext;
        }

        // Get User Details by User ID
        [HttpGet("/api/user/{userId}")]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            //var nameIdentifier = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            //user.Id != nameIdentifier
            if (user == null)
                return NotFound("User not found.");

            Console.WriteLine("t");
            var orders = await  _context.Orders.Include(o => o.OrdersDetails).Where(o => o.UserId == user.Id).ToListAsync();
            Console.WriteLine("v");

            return Ok(new
            {
                UserId = user.Id,
                Username = user.UserName,
                City = user.City,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Orders = orders.Select(o => new
                {
                    Id = o.Id,
                    Amount = o.Total_Amount_With_Discount,
                    Payment_Method = ""

                })
            });
        }

        [HttpPost("/api/user/delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            // Find the user by ID
            var userOrders = await _context.Orders
              .Where(o => o.UserId == userId)
              .ToListAsync();

            if (userOrders.Count > 0)
                return BadRequest("Cant Delete Active User");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found.");
            

            // Delete the user
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

     

            return Ok("User deleted successfully.");
        }

        //[Route("{productId:int}")]
        [HttpPost("/api/user/{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest userToUpdate, string id)
        {
            if (!ModelState.IsValid)
                return BadRequest("");

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return BadRequest("No user To Update");
            }

            user.PhoneNumber = userToUpdate.Phone_Number;
            user.UserName = userToUpdate.User_Name;

            var result =  await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User Updated Successfully");
        }

        [HttpGet("/api/user/users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1)
        {
            int pageSize = 10;

            // Get the total count of users
            var totalCount = await _userManager.Users.CountAsync();

            // Retrieve all users with related orders (paged)
            var usersList = await _userManager.Users
                .Skip((page - 1) * pageSize) // Skip records based on the page number
                .Take(pageSize)
                .Select(user => new GetUserResponse
                {
                    Id = user.Id,
                    User_Name = user.UserName,
                    Address = user.City,
                    Phone_Number = user.PhoneNumber,
                    Is_Confirmed = user.EmailConfirmed,
                    Email = user.Email,
                    Orders = _context.Orders
                        .Where(o => o.UserId == user.Id) // Filter orders by UserId
                        .Include(o => o.OrdersDetails) // Include related OrderDetails
                        .Select(o => new GetOrderResponse
                        {
                            Id = o.Id,
                            Order_Date = o.OrderDate,
                            Status_Ar = o.Status_Ar,
                            Status_En = o.Status_En,
                            Order_Details = o.OrdersDetails.Select(od => new GetOrderDetailResponse
                            {
                                Id = od.Id,
                                Product_Id = od.ProductId,
                                Unit_Price = od.Unit_Price,
                                Quantity_Of_Unit = od.Quantity_Of_Unit,
                                Offer = od.Offer,
                            }).ToList()
                        })
                        .ToList()
                })
                .ToListAsync();


            // Create a paginated response
            var pagedResponse = PagedList<GetUserResponse>.Create(usersList, page, pageSize, totalCount);

            return Ok(pagedResponse);
        }

    }
}