using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AlIssam.API.Dtos.Request
{
    public class ForgotPasswordRequest
    {
        [JsonConstructor]
        public ForgotPasswordRequest(
            string email,
            string clientUri)
        {
            Email = email;
            ClientUri = clientUri;
        }

        [Required(ErrorMessage = "Email is Required")]
        [JsonPropertyName("email")]
        public string Email { get; }

        [Required(ErrorMessage = "ClientUri is Required")]
        [JsonPropertyName("client_uri")]
        public string ClientUri { get; }
    }
}