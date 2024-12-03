using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Secuirty.Dtos;
using System;
using System.Threading.Tasks;

namespace Secuirty.MiddlerWares
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        public GlobalExceptionHandlingMiddleware(RequestDelegate request, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = request;
            _logger = logger;

        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                if (ex is FluentValidation.ValidationException validations)
                {
                    var response = new Response<string>
                    {
                        Message = string.Join(", ", validations.Errors),
                        StatusCode = StatusCodes.Status422UnprocessableEntity,

                    };
                    await context.Response.WriteAsync(response.ToString());
                }
                else
                {
                    var response = new Response<string>
                    {
                        Message = "Internal Server Error",
                        StatusCode = StatusCodes.Status500InternalServerError,

                    };
                    await context.Response.WriteAsync(response.ToString());

                }

            }
        }
    }
}
