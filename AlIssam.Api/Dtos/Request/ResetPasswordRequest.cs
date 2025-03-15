using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Request
{
    public class ResetPasswordRequest
    {
        [JsonConstructor]
        public ResetPasswordRequest(
            string password,
            string confirmPassword,
            string email,
            string token)
        {
            Password = password;
            ConfirmPassword = confirmPassword;
            Email = email;
            Token = token;
        }

        [Required(ErrorMessage = "Password is Required")]
        [JsonPropertyName("password")]
        public string Password { get; }

        [Required(ErrorMessage = "ConfirmPassword is Required")]
        //[Compare(nameof(Password), ErrorMessage = "ConfirmPassword should be identical to Password")]
        [JsonPropertyName("confirm_password")]
        public string ConfirmPassword { get; }

        [Required(ErrorMessage = "Email is Required")]
        [JsonPropertyName("email")]
        public string Email { get; }

        [Required(ErrorMessage = "Token is Required")]
        [JsonPropertyName("token")]
        public string Token { get; }
    }
}