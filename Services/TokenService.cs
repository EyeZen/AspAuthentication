using AspAuthentication.Data.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspAuthentication.Services
{
    public class TokenService
    {
        // specify how long until the token expires
        private const int ExpirationMinutes = 30;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _config;

        public TokenService(ILogger<TokenService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public string CreateToken(ApplicationUser user)
        {
            var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
            var token = CreateJwtToken(
                CreateClaims(user),
                CreateSigningCredentials(),
                expiration
            );
            var tokenHandler = new JwtSecurityTokenHandler();

            _logger.LogInformation("JWT Token created");

            return tokenHandler.WriteToken(token);
        }

        private JwtSecurityToken CreateJwtToken(
            List<Claim> claims,
            SigningCredentials credentials, DateTime expiration
        )
        {
            // Alternate way to access configurations
            //new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["ValidIssuer"],
            var validIssuer = _config.GetSection("JwtTokenSettings")["ValidIssuer"];
            var validAudience = _config.GetSection("JwtTokenSettings")["ValidAudience"];

            var token = new JwtSecurityToken(
                // Options parameters, passed in the payload of the token
                    validIssuer,
                    validAudience,
                    claims,
                    expires: expiration,
                    signingCredentials: credentials
                );

            return token;
        }

        private List<Claim> CreateClaims(ApplicationUser user)
        {
            //var jwtSub = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["JwtRegisterClaimNamesSub"];
            var jwtSub = _config.GetSection("JwtTokenSettings")["JwtRegisteredClaimNamesSub"];

            try
            {
                // TODO: Whats the difference between `JwtRegisteredClaimNames` and `ClaimTypes`? When to use which?
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, jwtSub!),                                                // Subject
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),                              // Token-Id
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),   // Issued-at
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };

                return claims;
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private SigningCredentials CreateSigningCredentials()
        {
            var symmetricSecurityKey = _config.GetSection("JwtTokenSettings")["SymmetricSecurityKey"];

            return new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(symmetricSecurityKey!)
                    ),
                    SecurityAlgorithms.HmacSha256
                );
        }
    }
}
