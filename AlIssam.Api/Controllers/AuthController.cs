using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Dtos.Response;
using AlIssam.API.Services.InterFaces;
using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.API.Controllers
{
    /// <summary>
    /// Handles user authentication and account management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly UserManager<User> _userManager;
        public AuthController(IIdentityService identityService, UserManager<User> userManager)
        {
            _identityService = identityService;
            _userManager = userManager;
        }

        /// <summary>
        /// Register new user account
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Confirmation result</returns>
        /// <response code="200">Registration successful (verification email sent)</response>
        /// <response code="400">Invalid registration data</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateNewUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            (bool IsSuccess, IEnumerable<string> Errors) = await _identityService.RegisterUser(request);
            if (Errors.Any() || !IsSuccess)
            {
                return BadRequest(Errors);
            }

            return Ok("User Created Successfully");
        }

        

        [HttpGet]
        [Route("email-confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {

            var (IsSuccess, ErrorMessages) = await _identityService.ConfirmMail(email, token);
            if (!IsSuccess)
                return BadRequest(ErrorMessages);

            return Ok("Confirmed Successfully");
        }

        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordRequest request)
        {

            var (IsSuccess, ErrorMessages) = await _identityService.ForgotPassword(request);
            if (!IsSuccess)
                return BadRequest(ErrorMessages);

            return Ok("Check Your Email Please");
        }


        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {

            var (IsSuccess, ErrorMessages) = await _identityService.ResetPassword(request);
            if (!IsSuccess)
                return BadRequest(ErrorMessages);

            return Ok("Password Reset Successfully");
        }

        /// <summary>
        /// Authenticate user and get JWT tokens
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication tokens</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="403">Email not verified</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loginResult = await _identityService.LogIn(request);

            if (!loginResult.IsSuccess)
            {
                return BadRequest(loginResult.ErrorMessages);
            }
            var user = await _userManager.FindByEmailAsync(request.Email);

            SetRefreshToken(loginResult.RefreshToken!);

            return Ok(new
            {
                token = loginResult.AccessToken,
                user = new
                {
                    Id = user.Id,
                    Email = user.Email,
                    User_Name = user.UserName,
                    Phone_Number = user.PhoneNumber,
                    Role = user.Role
                }

            });

        }

        [HttpGet]
        [Route("Refresh")]
        public async Task<IActionResult> RefreshToken([FromHeader] string authorization)
        {
            string refreshTokenVal = Request.Cookies.FirstOrDefault(x => x.Key == "refreshToken").Value;
            authorization = authorization.Split(" ")[1];

            var (IsSuccess, ErrorMessages, AccessToken) = await _identityService.RefreshToken(refreshTokenVal, authorization);

            if (!IsSuccess)
                return Unauthorized(ErrorMessages);

            return Ok(AccessToken);
        }

        /// <summary>
        /// Google OAuth login
        /// </summary>
        /// <param name="googleToken">Google ID token</param>
        /// <returns>JWT tokens and user details</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid token</response>
        /// 



        public class GoogleTokenRequest
        {
            public required string Token { get; set; }
        }

        
        [HttpPost]
        [Route("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest googleTokenRequest)
        {
            var googleAuthResult = await _identityService.LogInWithGoogle(googleTokenRequest.Token);

            if (!googleAuthResult.IsSuccess)
                return BadRequest(googleAuthResult.ErrorMessages);

            SetRefreshToken(googleAuthResult.RefreshToken!);
            //var user = await _userManager.FindByEmailAsync(userData.Email);

            return Ok(new
            {
                token = googleAuthResult.AccessToken,
                user = new
                {
                    Id = googleAuthResult.Response.Id,
                    Email = googleAuthResult.Response.Email,
                    User_Name = googleAuthResult.Response.User_Name,
                    Phone_Number = googleAuthResult.Response.Phone_Number,
                    Role = googleAuthResult.Response.Role
                }

            });
        }

        private void SetRefreshToken(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.ExpiresOn,
                Secure = true,
                Path = "/"
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            //Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5173/");

        }
    }
}
