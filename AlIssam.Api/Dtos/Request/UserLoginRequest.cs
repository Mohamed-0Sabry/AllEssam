using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Request
{
    public class UserLoginRequest
    {
        [JsonConstructor]
        public UserLoginRequest(
            string email,
            string password,
            string clientUri)
        {
            Email = email;
            Password = password;
            ClientUri = clientUri;
        }

        [Required(ErrorMessage = "client_uri is Required")]
        [JsonPropertyName("client_uri")]
        public string ClientUri { get; }

        [Required(ErrorMessage = "Email is Required")]
        [JsonPropertyName("email")]
        public string Email { get; }

        [Required(ErrorMessage = "Password is Required")]
        [JsonPropertyName("password")]
        public string Password { get; }
    }
}