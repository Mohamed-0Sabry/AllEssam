using Google.Apis.Auth;

namespace AlIssam.API.Services.InterFaces
{
    public interface IGoogleAuthService
    {
        public Task<GoogleJsonWebSignature.Payload> GetGoogleUserPayload(string token);

    }
}
