using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using AlIssam.API.Config;
using AlIssam.API.Services.InterFaces;

namespace AlIssam.API.Services
{
    /// <summary>
    /// Handles Google authentication integration
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthConfiguration _config;

        public GoogleAuthService(IOptions<GoogleAuthConfiguration> config)
        {
            _config = config.Value;
        }

        /// <summary>
        /// Validates a Google ID token and returns user payload
        /// </summary>
        /// <param name="token">Google ID token from client</param>
        /// <returns>Verified Google user information</returns>
        /// <exception cref="InvalidJwtException">Thrown for invalid tokens</exception>
        public async Task<GoogleJsonWebSignature.Payload> GetGoogleUserPayload(string token)
        {
            var googleUser = await GoogleJsonWebSignature.ValidateAsync(token,
              new GoogleJsonWebSignature.ValidationSettings()
              {
                  Audience = new[] { _config.ClientId },
              });
            return googleUser;
        }

    }
}
