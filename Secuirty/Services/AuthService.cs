
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Secuirty.Date;
using Secuirty.Dtos;
using Secuirty.Helper;
using Secuirty.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Secuirty.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManger;
        private readonly Jwt _jwt;
        private Context _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        public AuthService(
             UserManager<User> userManager,
             Context context,
             Jwt jwt,
             IEmailService emailService,
             ILogger<AuthService> logger
            )
        {
            _userManger = userManager;
            _jwt = jwt;
            _context = context;
            _emailService = emailService;
            _logger = logger;

        }
        private string GenerateRefreshToken()
        {
            var numbers = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(numbers);

            }
            return Convert.ToBase64String(numbers);
        }
        public async Task<Response<AutModel>> RefreshTokenAsync(RefreshTokenModel model)
        {
            try
            {
                var princple = GetClaimsPrincipalFromToken(model.AccessToken);
                var userId = princple?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {

                    return new Dtos.Response<AutModel>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                    };
                }
                var user = await _userManger.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                {
                    return new Dtos.Response<AutModel>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                    };

                }
                if (user.RefreshToken != model.RefreshToken)
                {
                    return new Dtos.Response<AutModel>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                    };

                }
                if (user.IsRevoked)
                    return new Dtos.Response<AutModel>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                    };
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(12);
                var accessToken = await CreateToken(user);
                var result = await _userManger.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AutModel>
                    {
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Something Bad Happen During Update User"
                    };
                }
                return new Response<AutModel>
                {
                    Data = new AutModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = user.RefreshToken,
                        RefreshTokenExpiryDate = user.RefreshTokenExpiryDate
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new Response<AutModel>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                };
            }


        }

        public ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
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

            var principal = handler.ValidateToken(token, tokenValidation, out SecurityToken outToken);

            return principal;
        }
        public async Task<Response<AutModel>> LoginAsync(LoginModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);


            if (user == null)
            {
                return new Response<AutModel>
                {
                    IsSuccess = false,
                    Message = "not is Exited",
                    StatusCode = (int)StatusCodes.Status401Unauthorized

                };
            }
            if (!await _userManger.CheckPasswordAsync(user, model.Password))
            {
                return new Response<AutModel>
                {
                    IsSuccess = false,
                    Message = "Username or Password is not valid",
                    StatusCode = (int)StatusCodes.Status401Unauthorized
                };
            }
            if (!user.EmailConfirmed)
                return new Response<AutModel>
                {
                    IsSuccess = false,
                    Message = "Username or Password is not valid",
                    StatusCode = (int)StatusCodes.Status401Unauthorized
                };

            if (user.IsRevoked)
            {
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(12);


                var result = await _userManger.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AutModel>
                    {
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Something Bad Happen During Update User"
                    };
                }
            }
            return new Response<AutModel>
            {
                IsSuccess = true,
                Data = new AutModel
                {
                    RefreshToken = user.RefreshToken,
                    RefreshTokenExpiryDate = user.RefreshTokenExpiryDate,
                    AccessToken = await CreateToken(user),

                },
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AutModel>> RegisterAsync(RegisterModel model)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {

                if (await _userManger.FindByNameAsync(model.UserName) is not null)
                    return new Response<AutModel>
                    {
                        IsSuccess = false,
                        Message = "Already existed",
                        StatusCode = 400

                    };
                if (await _userManger.FindByEmailAsync(model.Email) is not null)
                    return new Response<AutModel>
                    {
                        IsSuccess = false,
                        Message = "Already existed",
                        StatusCode = 400
                    };
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.UserName,

                };
                var result = await _userManger.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AutModel>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Something Bad Happen During Update User"
                    };
                }
                var addRoleUser = await _userManger.AddToRoleAsync(user, "User");
                if (!addRoleUser.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AutModel>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Something Bad Happen During Update User"

                    };
                }
                var confirmationToken = await GenerateConfirmationToken(user);
                await _emailService.SendAsync(new EmailModel
                {
                    Body = confirmationToken,
                    Subject = "Confirmation Email",
                    To = new string[] { user.Email }
                });
                var token = await CreateToken(user);

                await transaction.CommitAsync();
                return new Response<AutModel>
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Data = new AutModel
                    {

                        AccessToken = token,
                    }
                };

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new Response<AutModel>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    StatusCode = 500
                };
            }

        }

        private async Task<string> GenerateConfirmationToken(User user)
        {


            string token = await _userManger.GenerateEmailConfirmationTokenAsync(user);
            var queryDictionary = new Dictionary<string, string>
            {
                {"email",user.Email },
                {"token",token }
            };
            var confirmationMethod = $"{_jwt.Issuer}/api/Auth/ConfirmEmail";
            var link = QueryHelpers.AddQueryString(confirmationMethod, queryDictionary);



            return $"<a href={link}>{link}</a>";

        }

        private async Task<string> CreateToken(User user)
        {
            var userClaims = await _userManger.GetClaimsAsync(user);
            var roles = await _userManger.GetRolesAsync(user);
            var claimsRole = new List<Claim>();
            foreach (var role in roles)
            {
                claimsRole.Add(new Claim(ClaimTypes.Role, role));
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.GivenName,user.FirstName),
                new Claim(ClaimTypes.Surname,user.LastName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.UserName)
            }.Union(claimsRole).Union(userClaims);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var decriptor = new SecurityTokenDescriptor
            {
                Audience = _jwt.Audience,
                Issuer = _jwt.Issuer,
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddMinutes(_jwt.DurationInMin)

            };
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(decriptor);
            return handler.WriteToken(token);
        }

        public async Task<Response<string>> Revoke(string token)
        {
            var user = await _userManger.Users.FirstOrDefaultAsync(x => x.RefreshToken == token);
            if (user == null)
            {
                return new Response<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    IsSuccess = false,
                    Message = "user not found"
                };
            }
            if (user.IsRevoked)
            {
                return new Response<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    IsSuccess = false,
                    Message = "Already Revoked"

                };
            }
            user.RefreshToken = null;
            return new Response<string>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<string>> ConfirmEmail(ConfirmationModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return new Response<string>
                {
                    IsSuccess = false,
                    Message = "Invalid Email or Token",
                    StatusCode = 400
                };
            }
            var result = await _userManger.ConfirmEmailAsync(user, model.Token);


            if (!result.Succeeded)
            {
                _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));

                return new Response<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid Email or Token",
                };
            }
            return new Response<string>
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status200OK,
            };
        }

        public async Task<Response<string>> ResendConfirmationMessage(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new Response<string>
                {
                    IsSuccess = false,
                    Message = "Invalid Email"
                };
            }
            var user = await _userManger.FindByEmailAsync(email);
            if (user is null)
            {
                return new Response<string>
                {
                    IsSuccess = false,
                    Message = "Invalid Email"
                };
            }
            try
            {
                var token = await GenerateConfirmationToken(user);
                await _emailService.SendAsync(new EmailModel
                {
                    Subject = "Confirmation Email",
                    Body = token,
                    To = new[] { user.Email }
                });
                return new Response<string>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new Response<string>
                {
                    IsSuccess = false,
                    Message = "Smpt Sender is not working look at logging ",
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

        }
    }


}
