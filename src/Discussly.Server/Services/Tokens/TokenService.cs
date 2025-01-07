using Discussly.Server.Data.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Discussly.Server.Services.Tokens
{
    public class TokenService(IConfiguration config, UserManager<User> userManager) : Interfaces.ITokenService
    {
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config["JWT:SigningKey"]!));

        public async Task<string> CreateTokenAsync(User user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.NameId, user.Id),
                new (JwtRegisteredClaimNames.GivenName, user.UserName!),
                new (JwtRegisteredClaimNames.Email, user.Email!),
                new (ClaimTypes.Role, roles[0])
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddYears(1),
                SigningCredentials = creds,
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"],
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}