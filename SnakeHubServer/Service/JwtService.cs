using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SnakeHubServer.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SnakeHubServer.Service
{
    public class JwtService(UserManager<User> userManager, IConfiguration configuration)
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;
        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);
            IList<string> roles = await _userManager.GetRolesAsync(user);
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
                ]),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public ClaimsPrincipal GetClaimsFromJwtToken(string token)
        {
            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty))
            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }
    }
}
