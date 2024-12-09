using Microsoft.Extensions.DependencyInjection;
using Secuirty.BackGroundJobs;

namespace Secuirty.Extentions
{
    public static class InjectionClassExtention
    {
        public static IServiceCollection AddBackGroundTask(this IServiceCollection services)
        {
            services.AddHostedService<LoggingJob>();
            return services;
        }
    }
}
