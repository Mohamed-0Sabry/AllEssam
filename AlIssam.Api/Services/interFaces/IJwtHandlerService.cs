using AlIssam.DataAccessLayer.Entities;

namespace AlIssam.API.Services.InterFaces
{
    public interface IJwtHandlerService
    {
        public Task<(
            bool IsSuccess,
            RefreshToken? RefreshToken,
            string? AccessToken,
            IEnumerable<string>? ErrorMessages)>  HandleJwtTokensCreation(User user);

        public Task<(bool IsSuccess,
            IEnumerable<string>? ErrorMessages,
            string? AccessToken)>
            VerifyRefreshAndGenerateAccessAsync(string refreshToken, string accessToken);

    }
}
