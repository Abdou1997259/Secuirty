
using FluentValidation;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Secuirty.Commands;
using Secuirty.Date;
using Secuirty.Dtos;
using Secuirty.Helper;
using Secuirty.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly IValidator<RegisterModel> _userValidator;
        private readonly IValidator<RefreshTokenModel> _refreshToken;
        private readonly GoogleAuthConfig _googleOptions;
        private readonly HttpClient _facebookHttpClient;
        private readonly FaceBookAuthConfig _faceOptions;
        public AuthService(
             UserManager<User> userManager,
             Context context,
             Jwt jwt,
             IEmailService emailService,
             ILogger<AuthService> logger,
             IValidator<RegisterModel> userValidator,
             IValidator<RefreshTokenModel> refreshToken,
             IOptions<GoogleAuthConfig> googleOptions,
             IOptions<FaceBookAuthConfig> faceOptions

            )
        {
            _userManger = userManager;
            _jwt = jwt;
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _userValidator = userValidator;
            _refreshToken = refreshToken;
            _googleOptions = googleOptions.Value;
            _facebookHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/")
            };
            _faceOptions = faceOptions.Value;
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
        public async Task<Response<AuthModel>> RefreshTokenAsync(RefreshTokenModel model)
        {
            try
            {
                var refreshTokenValidator = await _refreshToken.ValidateAsync(model);
                if (!refreshTokenValidator.IsValid)
                {
                    return new Response<AuthModel>
                    {
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = string.Join(", ", refreshTokenValidator.Errors.Select(x => x.ErrorMessage))
                    };

                }


                var user = await _userManger.Users.FirstOrDefaultAsync(x => x.RefreshToken == model.RefreshToken);
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(12);
                var accessToken = await CreateToken(user);
                var result = await _userManger.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AuthModel>
                    {
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Something Bad Happen During Update User"
                    };
                }
                return new Response<AuthModel>
                {
                    Data = new AuthModel
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
                return new Response<AuthModel>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                };
            }


        }


        public async Task<Response<AuthModel>> LoginAsync(LoginModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);


            if (user == null)
            {
                return new Response<AuthModel>
                {
                    IsSuccess = false,
                    Message = "not is Exited",
                    StatusCode = (int)StatusCodes.Status401Unauthorized

                };
            }
            if (!await _userManger.CheckPasswordAsync(user, model.Password))
            {
                return new Response<AuthModel>
                {
                    IsSuccess = false,
                    Message = "Username or Password is not valid",
                    StatusCode = (int)StatusCodes.Status401Unauthorized
                };
            }
            if (!user.EmailConfirmed)
                return new Response<AuthModel>
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
                    return new Response<AuthModel>
                    {
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Something Bad Happen During Update User"
                    };
                }
            }
            return new Response<AuthModel>
            {
                IsSuccess = true,
                Data = new AuthModel
                {
                    RefreshToken = user.RefreshToken,
                    RefreshTokenExpiryDate = user.RefreshTokenExpiryDate,
                    AccessToken = await CreateToken(user),

                },
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AuthModel>> RegisterAsync(RegisterUserCommand model)
        {
            var transaction = _context.Database.BeginTransaction();
            try
            {



                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.UserName,
                    RefreshToken = GenerateRefreshToken(),
                    RefreshTokenExpiryDate = DateTime.UtcNow.Add(TimeSpan.FromDays(12))

                };
                var result = await _userManger.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AuthModel>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = string.Join(" , ", result.Errors.Select(x => x.Description))
                    };
                }
                var addRoleUser = await _userManger.AddToRoleAsync(user, "User");
                if (!addRoleUser.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AuthModel>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = string.Join(" , ", result.Errors.Select(x => x.Description))

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
                return new Response<AuthModel>
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Data = new AuthModel
                    {

                        AccessToken = token,
                        RefreshToken = user.RefreshToken,
                        RefreshTokenExpiryDate = user.RefreshTokenExpiryDate
                    }
                };

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new Response<AuthModel>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    StatusCode = 500
                };
            }

        }
        private async Task<string> GenerateResetPasswordAsync(User user)
        {


            string token = await _userManger.GeneratePasswordResetTokenAsync(user);
            var queryDictionary = new Dictionary<string, string>
            {
                {"email",user.Email },
                {"token",token }
            };
            var confirmationMethod = $"{_jwt.Audience}/auth/reset-password";
            var link = QueryHelpers.AddQueryString(confirmationMethod, queryDictionary);



            return $"<a href={link}>{link}</a>";

        }
        private async Task<string> GenerateConfirmationToken(User user)
        {


            string token = await _userManger.GenerateEmailConfirmationTokenAsync(user);
            var queryDictionary = new Dictionary<string, string>
            {
                {"email",user.Email },
                {"token",token }
            };
            var confirmationMethod = $"{_jwt.Audience}/auth/confirm";
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
                new Claim(ClaimTypes.Email,user.Email ?? ""),
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

        public async Task<Response<string>> ForgetPasswordAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return new Response<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid Email"
                };
            var user = await _userManger.FindByEmailAsync(email);
            if (user is null)
                return new Response<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid Email"
                };

            var token = await GenerateResetPasswordAsync(user);
            await _emailService.SendAsync(new EmailModel
            {
                Body = token,
                Subject = "ResetPassword",
                To = new[] { user.Email }
            });

            return new Response<string>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Reset Message sent please go to Reset Your Password"
            };






        }

        public async Task<Response<string>> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return new Response<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid Email"
                };
            }

            var result = await _userManger.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                return new Response<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = string.Join(" , ", result.Errors.Select(x => x.Description))
                };

            }
            return new Response<string>
            {

                IsSuccess = false,
                StatusCode = StatusCodes.Status200OK,
                Message = "Reset Password Achieved"
            };
        }

        public async Task<Response<AuthModel>> GoogleLoginAsync(string idToken)
        {
            var payload = await VerifyGoogleToken(idToken);
            if (payload == null)
            {
                return new Response<AuthModel>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid Google token"
                };
            }
            var user = await _userManger.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    EmailConfirmed = true
                };
                var createUserResult = await _userManger.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    return new Response<AuthModel>
                    {
                        IsSuccess = false,
                        Message = "Failed to create user",
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }

            }
            var token = await CreateToken(user);
            return new Response<AuthModel>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Data = new AuthModel
                {
                    AccessToken = token,
                    RefreshToken = user?.RefreshToken,
                    RefreshTokenExpiryDate = user.RefreshTokenExpiryDate
                }

            };
        }

        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _googleOptions.ClientId }
                };
                var payLoad = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payLoad;
            }
            catch
            {
                return null;
            }

        }
        private async Task<string> ValidateToken(string accessToken)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
                var tokenValidationParamter = new TokenValidationParameters
                {

                    IssuerSigningKey = key,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuer = _jwt.Issuer,
                    ValidAudience = _jwt.Audience,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var result = tokenHandler.ValidateToken(accessToken, tokenValidationParamter, out SecurityToken securityToken);
                return result?.FindFirst(ClaimTypes.NameIdentifier).Value;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }
        private async Task<bool> FacebookValidateAsync(string facebookToken, string userId)
        {

            var facebookKey = _faceOptions.AppId + "|" + _faceOptions.AppSecret;

            var fbResult = await _facebookHttpClient.GetFromJsonAsync<FaceResultDto>($"debug_token?input_token={facebookToken}&access_token={facebookKey}");
            if (fbResult == null || !fbResult.Data.Is_Valid || !fbResult.Data.User_Id.Equals(userId))
            {
                return false;

            }
            return true;

        }
        public async Task<Response<AuthModel>> RegsiterWithThirdParty(RegisterWithThirdPartyModel model)
        {

            if (model.Provider.Equals(SD.Facebook))
            {
                try
                {
                    if (!await FacebookValidateAsync(model.AccessToken, model.UserId))
                    {
                        return new Response<AuthModel>
                        {
                            IsSuccess = false,
                            Message = "invalid facebook access",
                            StatusCode = StatusCodes.Status401Unauthorized
                        };
                    }


                }
                catch (Exception ex)
                {
                    return new Response<AuthModel>
                    {
                        IsSuccess = false,
                        Message = "invalid facebook access",
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }

            }
            else if (model.Provider.Equals(SD.Google))
            {

                throw new NotImplementedException();
            }
            else
            {
                return new Response<AuthModel>()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    IsSuccess = false
                };

            }
            var user = await _userManger.FindByIdAsync(model.UserId);
            if (user is not null)
            {
                return new Response<AuthModel>
                {
                    IsSuccess = false,
                    Message = "Already created",
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            var userToAdd = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserId,
                Provider = model.Provider,
                RefreshTokenExpiryDate = DateTime.UtcNow.Add(TimeSpan.FromDays(12)),
                RefreshToken = GenerateRefreshToken(),
                Email = ""

            };
            try
            {
                var result = await _userManger.CreateAsync(userToAdd);


                var token = await CreateToken(userToAdd);
                return new Response<AuthModel>()
                {
                    IsSuccess = true,
                    Data = new AuthModel
                    {
                        AccessToken = token,
                        RefreshToken = userToAdd.RefreshToken,
                        RefreshTokenExpiryDate = userToAdd.RefreshTokenExpiryDate
                    },
                    StatusCode = StatusCodes.Status200OK
                };

            }
            catch (Exception ex)
            {
                return new Response<AuthModel>
                {
                    IsSuccess = false
                };
            }
        }

        public async Task<Response<AuthModel>> LoginWithThirdPartyAsync(LoginWithThirdPartyModel model)
        {
            try
            {

                if (model.Provider.Equals(SD.Facebook))
                {

                    if (!await FacebookValidateAsync(model.AccessToken, model.UserId))
                    {
                        return new Response<AuthModel>
                        {
                            Message = "invalid facebook login",
                            IsSuccess = false,
                            StatusCode = StatusCodes.Status401Unauthorized
                        };
                    }

                }


            }
            catch (Exception ex)
            {
                return new Response<AuthModel>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var user = await _userManger.Users.FirstOrDefaultAsync(x => x.UserName == model.UserId && x.Provider == model.Provider);
            if (user == null)
            {
                return new Response<AuthModel>
                {
                    Message = "invalid facebook account",
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            if (user.IsRevoked)
            {
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(12);


                var result = await _userManger.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError(string.Join(" , ", result.Errors.Select(x => x.Description)));
                    return new Response<AuthModel>
                    {
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Something Bad Happen During Update User"
                    };
                }
            }
            return new Response<AuthModel>
            {
                IsSuccess = true,
                Data = new AuthModel
                {
                    RefreshToken = user.RefreshToken,
                    RefreshTokenExpiryDate = user.RefreshTokenExpiryDate,
                    AccessToken = await CreateToken(user),

                },
                StatusCode = StatusCodes.Status200OK
            };
        }
    }


}
