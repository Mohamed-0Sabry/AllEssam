using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using AlIssam.API.Common;
using AlIssam.API.Dtos.Request;
using AlIssam.API.Services.InterFaces;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;
using System.Net;
using System.Security.Claims;
using AlIssam.Api.Dtos.Response;

namespace AlIssam.API.Services
{
    /// <summary>
    /// Manages user authentication, authorization, and account management
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IJwtHandlerService _jwtHandlerService;
        private readonly AlIssamDbContext _context;

        public IdentityService(
            UserManager<User> userManager,
            IEmailService emailService,
            IGoogleAuthService googleAuthService,
            IJwtHandlerService jwtHandlerService,
            AlIssamDbContext context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _googleAuthService = googleAuthService;
            _jwtHandlerService = jwtHandlerService;
            _context = context;
        }

        /// <summary>
        /// Registers a new user account and sends verification email
        /// </summary>
        /// <param name="userToCreate">User registration data</param>
        /// <returns>Tuple indicating success status and any error messages</returns>
        public async Task<(bool IsSuccess, IEnumerable<string> ErrorMessages)> RegisterUser(CreateNewUserRequest userToCreate)
        {
            var user = new User
            {
                UserName = userToCreate.UserName,
                Email = userToCreate.Email,
                City = userToCreate.City,
                Role = userToCreate.Role ?? "customer",
                PhoneNumber = userToCreate.Phone
            };

            var result = await _userManager.CreateAsync(user, userToCreate.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return (false, errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var param = new Dictionary<string, string>
            {
                {"token", encodedToken },
                {"email", user.Email!}
            };

            var callback = QueryHelpers.AddQueryString(userToCreate.ClientUri, param);
            var message = new Message(user.UserName, user.Email, "Email Confirmation Token", callback);
            await _emailService.SendEmail(message);

            return (true, Enumerable.Empty<string>());
        }

        /// <summary>
        /// Authenticates a user with email and password credentials
        /// </summary>
        /// <param name="user">Login request containing credentials</param>
        /// <returns>Tuple with auth status, JWT tokens, and error messages</returns>
        public async Task<(bool IsSuccess, IEnumerable<string>? ErrorMessages, string? AccessToken, RefreshToken? RefreshToken)>
            LogIn(UserLoginRequest user)
        {
            var foundUser = await _userManager.FindByEmailAsync(user.Email);
            if (foundUser is null || !await _userManager.CheckPasswordAsync(foundUser, user.Password))
                return (false, ["Email or Password is invalid"], null, null);

            if (!foundUser.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(foundUser);
                var param = new Dictionary<string, string>
                {
                    {"token", token },
                    {"email", user.Email!}
                };

                var callback = QueryHelpers.AddQueryString(user.ClientUri, param);
                var message = new Message(foundUser.UserName, user.Email, "Email Confirmation Token", callback);
                _emailService.SendEmail(message);
                return (false, ["Please verify your email"], null, null);
            }

            var generatedTokens = await _jwtHandlerService.HandleJwtTokensCreation(foundUser);

            if (!generatedTokens.IsSuccess)
                return (false, generatedTokens.ErrorMessages, null, null);

            return (true, null, generatedTokens.AccessToken, generatedTokens.RefreshToken);
        }

        /// <summary>
        /// Confirms user's email address using verification token
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="token">Verification token from email</param>
        /// <returns>Tuple indicating confirmation success status</returns>
        public async Task<(bool IsSuccess, IEnumerable<string>? ErrorMessages)> ConfirmMail(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, ["User not found"]);
            return (true, null);
        }

        /// <summary>
        /// Sends a password reset email
        /// </summary>
        public async Task<(bool IsSuccess, IEnumerable<string>? ErrorMessages)> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return (false, ["No such email"]);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var param = new Dictionary<string, string>
            {
                {"tokenReset", encodedToken },
                {"emailReset", user.Email!}
            };

            var callback = QueryHelpers.AddQueryString(request.ClientUri, param);
            var message = new Message(user.UserName, user.Email, "Reset Password Token", callback);
            _emailService.SendEmail(message);

            return (true, Enumerable.Empty<string>());
        }

        /// <summary>
        /// Resets the user's password using a token
        /// </summary>
        public async Task<(bool IsSuccess, IEnumerable<string>? ErrorMessages)> ResetPassword(ResetPasswordRequest requestData)
        {
            var user = await _userManager.FindByEmailAsync(requestData.Email);
            if (user == null)
                return (false, ["No such email"]);

            var decodedToken = WebUtility.UrlDecode(requestData.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, requestData.Password);

            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            return (true, null);
        }

        /// <summary>
        /// Logs in a user with Google authentication
        /// </summary>
        public async Task<(bool IsSuccess, IEnumerable<string>? ErrorMessages, LoginWithGoogleResponse? Response, RefreshToken? RefreshToken, string? AccessToken)>
       LogInWithGoogle(string token)
        {

            var googleUser = await _googleAuthService.GetGoogleUserPayload(token);
            if (googleUser == null)
            {
                return (false, new[] { "Invalid Google token" }, null, null, null);
            }

            var existingUser = await _userManager.FindByEmailAsync(googleUser.Email);
            if (existingUser != null)
            {
                var tokens = await _jwtHandlerService.HandleJwtTokensCreation(existingUser);
                if (!tokens.IsSuccess)
                {
                    return (false, tokens.ErrorMessages, null, null, null);
                }

                return (true, null, new LoginWithGoogleResponse
                {
                    User_Name = existingUser.UserName,
                    Id = existingUser.Id,
                    Email = existingUser.Email,
                    Phone_Number = existingUser.PhoneNumber,
                    Role = existingUser.Role
                }, tokens.RefreshToken, tokens.AccessToken);
            }

            var newUser = new User
            {
                UserName = googleUser.GivenName ?? googleUser.Email.Split('@')[0],
                Email = googleUser.Email,
                City = "Kuwait",
                Role = "customer",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                Console.WriteLine($"User creation failed: {string.Join(", ", errors)}");
                return (false, errors, null, null, null);
            }


            var userInfo = new UserLoginInfo("google", googleUser.Subject, "GOOGLE");
            var loginResult = await _userManager.AddLoginAsync(newUser, userInfo);
            if (!loginResult.Succeeded)
            {
                var errors = loginResult.Errors.Select(e => e.Description).ToList();
                Console.WriteLine($"Adding Google login failed: {string.Join(", ", errors)}");
                return (false, errors, null, null, null);
            }


            var generatedTokens = await _jwtHandlerService.HandleJwtTokensCreation(newUser);
            if (!generatedTokens.IsSuccess || string.IsNullOrEmpty(generatedTokens.AccessToken))
            {
                return (false, generatedTokens.ErrorMessages ?? new[] { "Failed to generate access token." }, null, null, null);
            }


            return (true, null, new LoginWithGoogleResponse
            {
                User_Name = newUser.UserName,
                Id = newUser.Id,
                Email = newUser.Email,
                Phone_Number = newUser.PhoneNumber,
                Role = newUser.Role
            }, generatedTokens.RefreshToken, generatedTokens.AccessToken);
        }


        public async Task<(bool IsSuccess, IEnumerable<string>? ErrorMessages, string? NewAccessToken)> RefreshToken(string refreshToken, string accessToken)
        {
            var result = await _jwtHandlerService.VerifyRefreshAndGenerateAccessAsync(refreshToken, accessToken);
            return result.IsSuccess ? (true, null, result.AccessToken) : (false, result.ErrorMessages, null);
        }
    }
}
