using Microsoft.AspNetCore.Builder;
using Secuirty.MiddlerWares;

namespace Secuirty.Extentions
{
    public static class MiddlewareExtentions
    {
        public static void UseCustomMiddleWares(this WebApplication app)
        {
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        }
    }
}
