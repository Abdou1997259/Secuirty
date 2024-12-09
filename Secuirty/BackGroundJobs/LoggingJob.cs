using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Secuirty.BackGroundJobs
{
    public class LoggingJob : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ILogger<LoggingJob> _logger;
        public LoggingJob(ILogger<LoggingJob> logger)
        {
            _logger = logger;


        }
        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timer = new Timer(Dowork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return Task.CompletedTask;

        }

        private void Dowork(object state)
        {
            _logger.LogInformation("Background job is running at: {time}", DateTimeOffset.Now);
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            _logger.LogInformation("LoggingJob stopped.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
