using LuminaBox.Application.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminaBox.Infrastructure.BackgroundJobs
{
    public class StallRentalStatusWorker : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StallRentalStatusWorker> _logger;

        public StallRentalStatusWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<StallRentalStatusWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IStallRentalService>();
                    var count = await service.SyncStatusesAsync();
                    if (count > 0)
                        _logger.LogInformation("Updated {Count} stall rentals/registrations.", count);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Failed to synchronize stall rental statuses.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}
