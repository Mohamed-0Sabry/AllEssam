using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Request
{
    public class CreateNewUserRequest
    {
        [JsonConstructor]
        public CreateNewUserRequest(
            string userName,
            string email,
            string city,
            string password,
            //string confirmPassword,
            string clientUri,
            string phone,
            string? role)
        {
            UserName = userName;
            Email = email;
            City = city;
            Password = password;
            Phone = phone;
            //ConfirmPassword = confirmPassword;
            ClientUri = clientUri;
            Role = role;
        }

        [Required(ErrorMessage = "UserName is Required")]
        [JsonPropertyName("username")]
        public string UserName { get; }

        [Required(ErrorMessage = "Email is Required")]
        [JsonPropertyName("email")]
        public string Email { get; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [Required(ErrorMessage = "Country is Required")]
        [JsonPropertyName("address")]
        public string City { get; }

        [Required(ErrorMessage = "Password is Required")]
        [JsonPropertyName("password")]
        public string Password { get; }

        [Required(ErrorMessage = "Phone is Required")]
        [JsonPropertyName("phone")]
        public string Phone { get; }


        [JsonPropertyName("client_uri")]
        public string ClientUri { get; }
    }
}