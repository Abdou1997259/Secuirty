using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Secuirty.Dtos;
using Secuirty.Helper;
using Secuirty.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Secuirty.Services
{
    public class ValidationService : IValidationService
    {
        private readonly UserManager<User> _userManager;
        private readonly Jwt _jwt;

        public ValidationService(UserManager<User> userManager, Jwt jwt)
        {
            _userManager = userManager;
            _jwt = jwt;
        }

        public async Task<bool> TokenValidating(RefreshTokenModel token)
        {
            var tokenValidation = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _jwt.Issuer,
                ValidAudience = _jwt.Audience,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key))
            };
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token.AccessToken, tokenValidation, out SecurityToken outToken);
                var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return false;
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;
                if (user.RefreshToken != token.RefreshToken)
                    return false;

                if (user.IsRevoked) return false;

                return true;
            }
            catch (Exception ex)
            {

                return false;

            }



        }

        public async Task<bool> UserExistenceByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email) is null;
        }

        public async Task<bool> UserExistenceByUserName(string userName)
        {
            return await _userManager.FindByNameAsync(userName) is null;
        }
    }
}
