using System.Text.Json.Serialization;

namespace AlIssam.Api.Dtos.Response
{
    public class LoginWithGoogleResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string User_Name { get; set; }
        public string Phone_Number { get; set; }
        public string Role { get; set; }

    }
}
