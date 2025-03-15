using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AlIssam.API.Config;
using AlIssam.API.Services.InterFaces;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AlIssam.API.Services
{
    public class JwtHandlerService : IJwtHandlerService
    {
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly UserManager<User> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly AlIssamDbContext _context;

        public JwtHandlerService(TokenValidationParameters tokenValidationParameters, UserManager<User> userManager, JwtConfig jwtConfig, AlIssamDbContext context)
        {
            _tokenValidationParameters = tokenValidationParameters;
            _userManager = userManager;
            _jwtConfig = jwtConfig;
            _context = context;
        }

        private string GenerateJwtToken(
            User userData, string securityJwtId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Auidiance,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Secret!)),
                    SecurityAlgorithms.HmacSha256),


                Subject = new ClaimsIdentity(new Claim[] {
                    new(ClaimTypes.NameIdentifier, userData.Id.ToString()),
                    new(ClaimTypes.Name, userData.UserName!),
                    new(ClaimTypes.Email, userData.Email!),
                    new(ClaimTypes.Role, userData.Role!),
                    new (JwtRegisteredClaimNames.Jti, securityJwtId),
                    new (JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),
                Expires = DateTime.UtcNow.Add(TimeSpan.Parse(_jwtConfig.ExpireTime!)),
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }

        private static string GenerateRandomString(int length)
        {
            var random = new Random();

            var chars = "ABCDEFGHIJKLMNOBQUSTUVWXYZ1234567890abcdefghijklmnopqrstuvwxyz_";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length - 1)]).ToArray());
            //return new string(Enumerable.Repeat(chars, length));
        }

        public async Task<(
            bool IsSuccess,
            RefreshToken? RefreshToken,
            string? AccessToken,
            IEnumerable<string>? ErrorMessages)> 
            HandleJwtTokensCreation(User user)
        {
            var securityTokenId = Guid.NewGuid().ToString();
            var accessToken = GenerateJwtToken(user, securityTokenId);

            if (string.IsNullOrEmpty(accessToken))
                return (false, null, null, ["Something Went Wrong"]);

            var refreshToken = new RefreshToken()
            {
                JwtId = securityTokenId,
                Token = GenerateRandomString(64),
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddMonths(6),
                RevokedOn = null,
                UserId = user.Id,
            };
            Console.WriteLine(user.Id);
            Console.WriteLine(user.Email);
            Console.WriteLine(refreshToken);


            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();
            return (true,refreshToken, accessToken, null);
        }

        public async Task<(bool IsSuccess,
            IEnumerable<string>? ErrorMessages,
            string? AccessToken)>
            VerifyRefreshAndGenerateAccessAsync(string refreshToken, string accessToken)
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    //_tokenValidationParameters.ValidateLifetime = false;

                    var tokenInVerification = jwtTokenHandler
                                    .ValidateToken(accessToken, _tokenValidationParameters, out var securityToken);

                    if (securityToken is JwtSecurityToken jwtSecurityToken)
                    {
                        var result = jwtSecurityToken.Header.Alg
                            .Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase);

                        if (result == false)
                            return (false, ["Provided Token Is Broken"], null);
                    }


                    var providedjti = tokenInVerification.Claims
                            .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    var (isValidToken, storedRefreshToken) = await IsValidRefreshTokenAsync(refreshToken);


                    if (isValidToken || ValidateRefreshTokenJti(storedRefreshToken!, providedjti!) )
                        return (false, ["Session Expired Please re-login"], null);

                    await _context.SaveChangesAsync();

                    var user = await _userManager.FindByIdAsync(storedRefreshToken.UserId);

                    return (false,
                             null,
                             AccessToken: GenerateJwtToken(user, storedRefreshToken.JwtId)
                           );

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
        }
        private  async Task<(bool IsValid, RefreshToken? storedToken)> IsValidRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (storedToken == null || !storedToken.IsActive)
                return (false, null);
            return (true, storedToken);
        }

        private static  bool ValidateRefreshTokenJti (RefreshToken storedToken, string providedJti)
        {
            if (storedToken is null || storedToken.JwtId != providedJti)
                return false;
            return true;
        }
    }
}
